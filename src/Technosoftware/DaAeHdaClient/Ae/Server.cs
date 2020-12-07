#region Copyright (c) 2011-2020 Technosoftware GmbH. All rights reserved
//-----------------------------------------------------------------------------
// Copyright (c) 2011-2020 Technosoftware GmbH. All rights reserved
// Web: https://www.technosoftware.com 
// 
// The source code in this file is covered under a dual-license scenario:
//   - Owner of a purchased license: SCLA 1.0
//   - GPL V3: everybody else
//
// SCLA license terms accompanied with this source code.
// See SCLA 1.0://technosoftware.com/license/Source_Code_License_Agreement.pdf
//
// GNU General Public License as published by the Free Software Foundation;
// version 3 of the License are accompanied with this source code.
// See https://technosoftware.com/license/GPLv3License.txt
//
// This source code is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
// or FITNESS FOR A PARTICULAR PURPOSE.
//-----------------------------------------------------------------------------
#endregion Copyright (c) 2011-2020 Technosoftware GmbH. All rights reserved

#region Using Directives
using System;
using System.Collections;
using System.Runtime.Serialization;
#endregion

namespace Technosoftware.DaAeHdaClient.Ae
{
	/// <summary>
	/// An in-process object which provides access to AE server objects.
	/// </summary>
	[Serializable]
	public class TsCAeServer : Technosoftware.DaAeHdaClient.OpcServer, ITsCAeServer, ISerializable
	{
		///////////////////////////////////////////////////////////////////////
		#region SubscriptionCollection Class

		/// <summary>
		/// A read-only collection of subscriptions.
		/// </summary>
		public class SubscriptionCollection : OpcReadOnlyCollection
		{
			///////////////////////////////////////////////////////////////////
			#region Constructors, Destructor, Initialization

			/// <summary>
			/// Creates an empty collection.
			/// </summary>
			internal SubscriptionCollection() : base(new Ae.TsCAeSubscription[0]) { }

			#endregion

			///////////////////////////////////////////////////////////////////
			#region Public Methods

			/// <summary>
			/// An indexer for the collection.
			/// </summary>
			public new TsCAeSubscription this[int index]
			{
				get { return (TsCAeSubscription)Array.GetValue(index); }
			}

			/// <summary>
			/// Returns a copy of the collection as an array.
			/// </summary>
			public new TsCAeSubscription[] ToArray()
			{
				return (TsCAeSubscription[])Array;
			}

			/// <summary>
			/// Adds a subscription to the end of the collection.
			/// </summary>
			internal void Add(Ae.TsCAeSubscription subscription)
			{
				TsCAeSubscription[] array = new TsCAeSubscription[Count + 1];

				Array.CopyTo(array, 0);
				array[Count] = subscription;

				Array = array;
			}

			/// <summary>
			/// Removes a subscription to the from the collection.
			/// </summary>
			internal void Remove(Ae.TsCAeSubscription subscription)
			{
				TsCAeSubscription[] array = new TsCAeSubscription[Count - 1];

				int index = 0;

				for (int ii = 0; ii < Array.Length; ii++)
				{
					TsCAeSubscription element = (TsCAeSubscription)Array.GetValue(ii);

					if (subscription != element)
					{
						array[index++] = element;
					}
				}

				Array = array;
			}

			#endregion
		}

		#endregion

		///////////////////////////////////////////////////////////////////////
		#region Names Class

		/// <summary>
		/// A set of names for fields used in serialization.
		/// </summary>
		private class Names
		{
			internal const string COUNT = "CT";
			internal const string SUBSCRIPTION = "SU";
		}

		#endregion

		///////////////////////////////////////////////////////////////////////
		#region Fields

		private int _filters;
		private bool _disposing;
		private SubscriptionCollection _subscriptions = new SubscriptionCollection();

		#endregion

		///////////////////////////////////////////////////////////////////////
		#region Constructors, Destructor, Initialization

        /// <summary>
        /// Initializes the object.
        /// </summary>
        public TsCAeServer()
            
        {
            base._factory = new Com.Factory();
        }

		/// <summary>
		/// Initializes the object with a factory and a default OpcUrl.
		/// </summary>
		/// <param name="factory">The OpcFactory used to connect to remote servers.</param>
		/// <param name="url">The network address of a remote server.</param>
		public TsCAeServer(OpcFactory factory, OpcUrl url)
			: base(factory, url)
		{
		}

		/// <summary>
		/// Contructs a server by de-serializing its OpcUrl from the stream.
		/// </summary>
		protected TsCAeServer(SerializationInfo info, StreamingContext context)
			:
			base(info, context)
		{
			int count = (int)info.GetValue(Names.COUNT, typeof(int));

			_subscriptions = new SubscriptionCollection();

			for (int ii = 0; ii < count; ii++)
			{
				Ae.TsCAeSubscription subscription = (Ae.TsCAeSubscription)info.GetValue(Names.SUBSCRIPTION + ii.ToString(), typeof(Ae.TsCAeSubscription));
				_subscriptions.Add(subscription);
			}
		}

		#endregion

		///////////////////////////////////////////////////////////////////////
		#region Properties

		/// <summary>
		/// The filters supported by the server.
		/// </summary>
		public int AvailableFiltesr
		{
			get { return _filters; }
		}

		/// <summary>
		/// The outstanding subscriptions for the server.
		/// </summary>
		public SubscriptionCollection Subscriptions
		{
			get { return _subscriptions; }
		}
        #endregion

		///////////////////////////////////////////////////////////////////////
		#region Public Methods

		/// <summary>
		/// Connects to the server with the specified OpcUrl and credentials.
		/// </summary>
		public override void Connect(OpcUrl url, OpcConnectData connectData)
		{
            if (_factory == null)
			{
				_factory = new Com.Factory();
			}
			// connect to server.
			base.Connect(url, connectData);

			// all done if no subscriptions.
			if (_subscriptions.Count == 0)
			{
				return;
			}

			// create subscriptions (should only happen if server has been deserialized).
			SubscriptionCollection subscriptions = new SubscriptionCollection();

			foreach (Ae.TsCAeSubscription template in _subscriptions)
			{
				// create subscription for template.
				try { subscriptions.Add(EstablishSubscription(template)); }
				catch { }
			}

			// save new set of subscriptions.
			_subscriptions = subscriptions;
		}

		/// <summary>
		/// Disconnects from the server and releases all network resources.
		/// </summary>
		public override void Disconnect()
		{
            if (_server == null) throw new NotConnectedException();

			// dispose of all subscriptions first.
			_disposing = true;

			foreach (Ae.TsCAeSubscription subscription in _subscriptions)
			{
				subscription.Dispose();
			}

			_disposing = false;

			// disconnect from server.
			base.Disconnect();
		}

        /// <summary>
        /// Returns the current server status.
        /// </summary>
        /// <returns>The current server status.</returns>
        public OpcServerStatus GetServerStatus()
        {
            if (_server == null) throw new NotConnectedException();

            OpcServerStatus status = ((ITsCAeServer)_server).GetServerStatus();

            if (status != null)
            {
                if (status.StatusInfo == null)
                {
                    status.StatusInfo = GetString(String.Format("serverState.{0}", status.ServerState));
                }
            }
            else
            {
                if (_server == null) throw new NotConnectedException();
            }

            return status;
        }

		/// <summary>
		/// Creates a new event subscription.
		/// </summary>
		/// <param name="state">The initial state for the subscription.</param>
		/// <returns>The new subscription object.</returns>
		public ITsCAeSubscription CreateSubscription(Ae.TsCAeSubscriptionState state)
		{
            if (_server == null) throw new NotConnectedException();

            // create remote object.
            ITsCAeSubscription subscription = ((ITsCAeServer)_server).CreateSubscription(state);

			if (subscription != null)
			{
				// create wrapper.
				Ae.TsCAeSubscription wrapper = new Ae.TsCAeSubscription(this, subscription, state);
				_subscriptions.Add(wrapper);
				return wrapper;
			}

			// should never happen.
			return null;
		}

		/// <summary>
		/// Returns the event filters supported by the server.
		/// </summary>
		/// <returns>A bit mask of all event filters supported by the server.</returns>
		public int QueryAvailableFilters()
		{
            if (_server == null) throw new NotConnectedException();

            _filters = ((ITsCAeServer)_server).QueryAvailableFilters();

			return _filters;
		}

		/// <summary>       
		/// Returns the event categories supported by the server for the specified event types.
		/// </summary>
		/// <param name="eventType">A bit mask for the event types of interest.</param>
		/// <returns>A collection of event categories.</returns>
		public Technosoftware.DaAeHdaClient.Ae.TsCAeCategory[] QueryEventCategories(int eventType)
		{
            if (_server == null) throw new NotConnectedException();

            // fetch categories from server.
            Technosoftware.DaAeHdaClient.Ae.TsCAeCategory[] categories = ((ITsCAeServer)_server).QueryEventCategories(eventType);

			// return result.
			return categories;
		}

		/// <summary>
		/// Returns the condition names supported by the server for the specified event categories.
		/// </summary>
		/// <param name="eventCategory">A bit mask for the event categories of interest.</param>
		/// <returns>A list of condition names.</returns>
		public string[] QueryConditionNames(int eventCategory)
		{
            if (_server == null) throw new NotConnectedException();

            // fetch condition names from the server.
            string[] conditions = ((ITsCAeServer)_server).QueryConditionNames(eventCategory);

			// return result.
			return conditions;
		}

		/// <summary>
		/// Returns the sub-condition names supported by the server for the specified event condition.
		/// </summary>
		/// <param name="conditionName">The name of the condition.</param>
		/// <returns>A list of sub-condition names.</returns>
		public string[] QuerySubConditionNames(string conditionName)
		{
            if (_server == null) throw new NotConnectedException();

            // fetch sub-condition names from the server.
            string[] subconditions = ((ITsCAeServer)_server).QuerySubConditionNames(conditionName);

			// return result.
			return subconditions;
		}

		/// <summary>
		/// Returns the condition names supported by the server for the specified event source.
		/// </summary>
		/// <param name="sourceName">The name of the event source.</param>
		/// <returns>A list of condition names.</returns>
		public string[] QueryConditionNames(string sourceName)
		{
            if (_server == null) throw new NotConnectedException();

            // fetch condition names from the server.
            string[] conditions = ((ITsCAeServer)_server).QueryConditionNames(sourceName);

			// return result.
			return conditions;
		}

		/// <summary>       
		/// Returns the event attributes supported by the server for the specified event categories.
		/// </summary>
		/// <param name="eventCategory">A bit mask for the event categories of interest.</param>
		/// <returns>A collection of event attributes.</returns>
		public Technosoftware.DaAeHdaClient.Ae.TsCAeAttribute[] QueryEventAttributes(int eventCategory)
		{
            if (_server == null) throw new NotConnectedException();

            // fetch attributes from server.
            Technosoftware.DaAeHdaClient.Ae.TsCAeAttribute[] attributes = ((ITsCAeServer)_server).QueryEventAttributes(eventCategory);

			// return result.
			return attributes;
		}

		/// <summary>
		/// Returns the DA item ids for a set of attribute ids belonging to events which meet the specified filter criteria.
		/// </summary>
		/// <param name="sourceName">The event source of interest.</param>
		/// <param name="eventCategory">The id of the event category for the events of interest.</param>
		/// <param name="conditionName">The name of a condition within the event category.</param>
		/// <param name="subConditionName">The name of a sub-condition within a multi-state condition.</param>
		/// <param name="attributeIDs">The ids of the attributes to return item ids for.</param>
		/// <returns>A list of item urls for each specified attribute.</returns>
		public TsCAeItemUrl[] TranslateToItemIDs(
		  string sourceName,
		  int eventCategory,
		  string conditionName,
		  string subConditionName,
		  int[] attributeIDs)
		{
            if (_server == null) throw new NotConnectedException();

            TsCAeItemUrl[] itemUrls = ((ITsCAeServer)_server).TranslateToItemIDs(
				sourceName,
				eventCategory,
				conditionName,
				subConditionName,
				attributeIDs);

			return itemUrls;
		}

		/// <summary>
		/// Returns the current state information for the condition instance corresponding to the source and condition name.
		/// </summary>
		/// <param name="sourceName">The source name</param>
		/// <param name="conditionName">A condition name for the source.</param>
		/// <param name="attributeIDs">The list of attributes to return with the condition state.</param>
		/// <returns>The current state of the connection.</returns>
		public Ae.TsCAeCondition GetConditionState(
			string sourceName,
			string conditionName,
			int[] attributeIDs)
		{
            if (_server == null) throw new NotConnectedException();

            Ae.TsCAeCondition condition = ((ITsCAeServer)_server).GetConditionState(sourceName, conditionName, attributeIDs);

			return condition;
		}

		/// <summary>
		/// Places the specified process areas into the enabled state.
		/// </summary>
		/// <param name="areas">A list of fully qualified area names.</param>
		/// <returns>The results of the operation for each area.</returns>
		public OpcResult[] EnableConditionByArea(string[] areas)
		{
            if (_server == null) throw new NotConnectedException();

            OpcResult[] results = ((ITsCAeServer)_server).EnableConditionByArea(areas);

			return results;
		}

		/// <summary>
		/// Places the specified process areas into the disabled state.
		/// </summary>
		/// <param name="areas">A list of fully qualified area names.</param>
		/// <returns>The results of the operation for each area.</returns>
		public OpcResult[] DisableConditionByArea(string[] areas)
		{
            if (_server == null) throw new NotConnectedException();

            OpcResult[] results = ((ITsCAeServer)_server).DisableConditionByArea(areas);

			return results;
		}

		/// <summary>
		/// Places the specified process areas into the enabled state.
		/// </summary>
		/// <param name="sources">A list of fully qualified source names.</param>
		/// <returns>The results of the operation for each area.</returns>
		public OpcResult[] EnableConditionBySource(string[] sources)
		{
            if (_server == null) throw new NotConnectedException();

            OpcResult[] results = ((ITsCAeServer)_server).EnableConditionBySource(sources);

			return results;
		}

		/// <summary>
		/// Places the specified process areas into the disabled state.
		/// </summary>
		/// <param name="sources">A list of fully qualified source names.</param>
		/// <returns>The results of the operation for each area.</returns>
		public OpcResult[] DisableConditionBySource(string[] sources)
		{
            if (_server == null) throw new NotConnectedException();

            OpcResult[] results = ((ITsCAeServer)_server).DisableConditionBySource(sources);

			return results;
		}

		/// <summary>
		/// Returns the enabled state for the specified process areas. 
		/// </summary>
		/// <param name="areas">A list of fully qualified area names.</param>
		public TsCAeEnabledStateResult[] GetEnableStateByArea(string[] areas)
		{
            if (_server == null) throw new NotConnectedException();

            TsCAeEnabledStateResult[] results = ((ITsCAeServer)_server).GetEnableStateByArea(areas);

			return results;
		}

		/// <summary>
		/// Returns the enabled state for the specified event sources. 
		/// </summary>
		/// <param name="sources">A list of fully qualified source names.</param>
		public TsCAeEnabledStateResult[] GetEnableStateBySource(string[] sources)
		{
            if (_server == null) throw new NotConnectedException();

            TsCAeEnabledStateResult[] results = ((ITsCAeServer)_server).GetEnableStateBySource(sources);

			return results;
		}

		/// <summary>
		/// Used to acknowledge one or more conditions in the event server.
		/// </summary>
		/// <param name="acknowledgerID">The identifier for who is acknowledging the condition.</param>
		/// <param name="comment">A comment associated with the acknowledgment.</param>
		/// <param name="conditions">The conditions being acknowledged.</param>
		/// <returns>A list of result id indictaing whether each condition was successfully acknowledged.</returns>
		public OpcResult[] AcknowledgeCondition(
			string acknowledgerID,
			string comment,
			TsCAeEventAcknowledgement[] conditions)
		{
            if (_server == null) throw new NotConnectedException();

            return ((ITsCAeServer)_server).AcknowledgeCondition(acknowledgerID, comment, conditions);
		}

		/// <summary>
		/// Browses for all children of the specified area that meet the filter criteria.
		/// </summary>
		/// <param name="areaID">The full-qualified id for the area.</param>
		/// <param name="browseType">The type of children to return.</param>
		/// <param name="browseFilter">The expression used to filter the names of children returned.</param>
		/// <returns>The set of elements that meet the filter criteria.</returns>
		public Technosoftware.DaAeHdaClient.Ae.TsCAeBrowseElement[] Browse(
			string areaID,
			Technosoftware.DaAeHdaClient.Ae.TsCAeBrowseType browseType,
			string browseFilter)
		{
            if (_server == null) throw new NotConnectedException();

            return ((ITsCAeServer)_server).Browse(areaID, browseType, browseFilter);
		}

		/// <summary>
		/// Browses for all children of the specified area that meet the filter criteria.
		/// </summary>
		/// <param name="areaID">The full-qualified id for the area.</param>
		/// <param name="browseType">The type of children to return.</param>
		/// <param name="browseFilter">The expression used to filter the names of children returned.</param>
		/// <param name="maxElements">The maximum number of elements to return.</param>
		/// <param name="position">The object used to continue the browse if the number nodes exceeds the maximum specified.</param>
		/// <returns>The set of elements that meet the filter criteria.</returns>
		public Technosoftware.DaAeHdaClient.Ae.TsCAeBrowseElement[] Browse(
			string areaID,
			Technosoftware.DaAeHdaClient.Ae.TsCAeBrowseType browseType,
			string browseFilter,
			int maxElements,
			out IOpcBrowsePosition position)
		{
            if (_server == null) throw new NotConnectedException();

            return ((ITsCAeServer)_server).Browse(areaID, browseType, browseFilter, maxElements, out position);
		}

		/// <summary>
		/// Continues browsing the server's address space at the specified position.
		/// </summary>
		/// <param name="maxElements">The maximum number of elements to return.</param>
		/// <param name="position">The position object used to continue a browse operation.</param>
		/// <returns>The set of elements that meet the filter criteria.</returns>
		public Technosoftware.DaAeHdaClient.Ae.TsCAeBrowseElement[] BrowseNext(int maxElements, ref IOpcBrowsePosition position)
		{
            if (_server == null) throw new NotConnectedException();

            return ((ITsCAeServer)_server).BrowseNext(maxElements, ref position);
		}

		#endregion

		///////////////////////////////////////////////////////////////////////
		#region ISerializable Members

		/// <summary>
		/// Serializes a server into a stream.
		/// </summary>
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);

			info.AddValue(Names.COUNT, _subscriptions.Count);

			for (int ii = 0; ii < _subscriptions.Count; ii++)
			{
				info.AddValue(Names.SUBSCRIPTION + ii.ToString(), _subscriptions[ii]);
			}
		}

		#endregion

		///////////////////////////////////////////////////////////////////////
		#region Private Methods

		/// <summary>
		/// Called when a subscription object is disposed.
		/// </summary>
		/// <param name="subscription"></param>
		internal void SubscriptionDisposed(Ae.TsCAeSubscription subscription)
		{
			if (!_disposing)
			{
				_subscriptions.Remove(subscription);
			}
		}

		/// <summary>
		/// Establishes a subscription based on the template provided.
		/// </summary>
		private Ae.TsCAeSubscription EstablishSubscription(Ae.TsCAeSubscription template)
		{
			ITsCAeSubscription remoteServer = null;

			try
			{
				// create remote object.
				remoteServer = ((ITsCAeServer)_server).CreateSubscription(template.State);

				if (remoteServer == null)
				{
					return null;
				}

				// create wrapper.
				Ae.TsCAeSubscription subscription = new Ae.TsCAeSubscription(this, remoteServer, template.State);

				// set filters.
				subscription.SetFilters(template.Filters);

				// set attributes.
				IDictionaryEnumerator enumerator = template.Attributes.GetEnumerator();

				while (enumerator.MoveNext())
				{
					subscription.SelectReturnedAttributes(
						(int)enumerator.Key,
						((Ae.TsCAeSubscription.AttributeCollection)enumerator.Value).ToArray());
				}

				// return new subscription
				return subscription;
			}
			catch
			{
				if (remoteServer != null)
				{
					remoteServer.Dispose();
					remoteServer = null;
				}
			}

			// return null.
			return null;
		}

		#endregion

	}
}
