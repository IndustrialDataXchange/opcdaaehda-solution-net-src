#region Copyright (c) 2011-2020 Technosoftware GmbH. All rights reserved
//-----------------------------------------------------------------------------
// Copyright (c) 2011-2020 Technosoftware GmbH. All rights reserved
// Web: https://www.technosoftware.com 
// 
// The source code in this file is covered under a dual-license scenario:
//   - Owner of a purchased license: RPL 1.5
//   - GPL V3: everybody else
//
// RPL license terms accompanied with this source code.
// See https://technosoftware.com/license/RPLv15License.txt
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

namespace Technosoftware.DaAeHdaClient.Da
{
    /// <summary>
    /// An in-process object used to access subscriptions on OPC Data Access servers.
    /// </summary>
    [Serializable]
    public class TsCDaSubscription : ITsCDaSubscription, IDisposable, ISerializable, ICloneable
    {
        ///////////////////////////////////////////////////////////////////////
        #region Names Class

        /// <summary>
        /// A set of names for fields used in serialization.
        /// </summary>
        private class Names
        {
            internal const string STATE = "State";
            internal const string FILTERS = "Filters";
            internal const string ITEMS = "Items";
        }

        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region Fields

        private bool _disposed;

        /// <summary>
        /// The containing server object.
        /// </summary>
        internal TsCDaServer _server;

        /// <summary>
        /// The remote subscription object.
        /// </summary>
        internal ITsCDaSubscription _subscription;

        /// <summary>
        /// The local copy of the subscription state.
        /// </summary>
        private TsCDaSubscriptionState _state = new TsCDaSubscriptionState();

        /// <summary>
        /// The local copy of all subscription items.
        /// </summary>
        private TsCDaItem[] _items;

        /// <summary>
        /// Whether data callbacks are enabled.
        /// </summary>
        private bool _enabled = true;

        /// <summary>
        /// The local copy of the result filters.
        /// </summary>
        private int _filters = (int)TsCDaResultFilter.All | (int)TsCDaResultFilter.ClientHandle;

        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region Constructors, Destructor, Initialization

        /// <summary>
        /// Initializes object with default values.
        /// </summary>
        public TsCDaSubscription(TsCDaServer server, ITsCDaSubscription subscription)
        {
            if (server == null)
            {
                throw new ArgumentNullException("server");
            }
            if (subscription == null)
            {
                throw new ArgumentNullException("subscription");
            }

            _server = server;
            _subscription = subscription;

            GetResultFilters();
            GetState();
        }

        /// <summary>
        /// Contructs a server by de-serializing its OpcUrl from the stream.
        /// </summary>
        protected TsCDaSubscription(SerializationInfo info, StreamingContext context)
        {
            _state = (TsCDaSubscriptionState)info.GetValue(Names.STATE, typeof(TsCDaSubscriptionState));
            _filters = (int)info.GetValue(Names.FILTERS, typeof(int));
            _items = (TsCDaItem[])info.GetValue(Names.ITEMS, typeof(TsCDaItem[]));
        }

        /// <summary>
        /// The finializer implementation.
        /// </summary>
        ~TsCDaSubscription()
        {
            Dispose(false);
        }

        /// <summary>
        /// This must be called explicitly by clients to ensure the remote server is released.
        /// </summary>
        public virtual void Dispose()
        {
            Dispose(true);
            // Take yourself off the Finalization queue
            // to prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose(bool disposing) executes in two distinct scenarios.
        /// If disposing equals true, the method has been called directly
        /// or indirectly by a user's code. Managed and unmanaged resources
        /// can be disposed.
        /// If disposing equals false, the method has been called by the
        /// runtime from inside the finalizer and you should not reference
        /// other objects. Only unmanaged resources can be disposed.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    if (_subscription != null)
                    {
                        _subscription.Dispose();

                        _server = null;
                        _subscription = null;
                        _items = null;
                    }
                }
                // Release unmanaged resources. If disposing is false,
                // only the following code is executed.
            }
            _disposed = true;
        }

        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region Properties

        /// <summary>
        /// The server that the subscription is attached to.
        /// </summary>
        public TsCDaServer Server { get { return _server; } }

        /// <summary>
        /// The name assigned to the subscription by the client.
        /// </summary>
        public string Name { get { return _state.Name; } }

        /// <summary>
        /// The handle assigned to the subscription by the client.
        /// </summary>
        public object ClientHandle { get { return _state.ClientHandle; } }

        /// <summary>
        /// The handle assigned to the subscription by the server.
        /// </summary>
        public object ServerHandle { get { return _state.ServerHandle; } }

        /// <summary>
        /// Whether the subscription is active.
        /// </summary>
        public bool Active { get { return _state.Active; } }

        /// <summary>
        /// Whether data callbacks are enabled.
        /// </summary>
        public bool Enabled { get { return _enabled; } }

        /// <summary>
        /// The current locale used by the subscription.
        /// </summary>
        public string Locale { get { return _state.Locale; } }

        /// <summary>
        /// The current result filters applied by the subscription.
        /// </summary>
        public int Filters { get { return _filters; } }

        /// <summary>
        /// Returns a copy of the current subscription state.
        /// </summary>
        public TsCDaSubscriptionState State { get { return (TsCDaSubscriptionState)_state.Clone(); } }

        /// <summary>
        /// The items belonging to the subscription.
        /// </summary>
        public TsCDaItem[] Items {
            get {
                if (_items == null) return new TsCDaItem[0];
                TsCDaItem[] items = new TsCDaItem[_items.Length];
                for (int ii = 0; ii < _items.Length; ii++) items[ii] = (TsCDaItem)_items[ii].Clone();
                return items;
            }
        }

        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region Public Methods

        /// <summary>
        /// Serializes a server into a stream.
        /// </summary>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(Names.STATE, _state);
            info.AddValue(Names.FILTERS, _filters);
            info.AddValue(Names.ITEMS, _items);
        }

        /// <summary>
        /// Returns an unconnected copy of the subscription with the same items.
        /// </summary>
        public virtual object Clone()
        {
            // do a memberwise clone.
            TsCDaSubscription clone = (TsCDaSubscription)MemberwiseClone();

            // place clone in disconnected state.
            clone._server = null;
            clone._subscription = null;
            clone._state = (TsCDaSubscriptionState)_state.Clone();

            // clear server handles.
            clone._state.ServerHandle = null;

            // always make cloned subscriptions inactive.
            clone._state.Active = false;

            // clone items.
            if (clone._items != null)
            {
                ArrayList items = new ArrayList();

                Array.ForEach(clone._items, item => items.Add(item.Clone()));

                clone._items = (TsCDaItem[])items.ToArray(typeof(TsCDaItem));
            }

            // return clone.
            return clone;
        }

        /// <summary>
        /// Gets default result filters for the server.
        /// </summary>
        public int GetResultFilters()
        {
            _filters = _subscription.GetResultFilters();
            return _filters;
        }

        /// <summary>
        /// Sets default result filters for the server.
        /// </summary>
        public void SetResultFilters(int filters)
        {
            _subscription.SetResultFilters(filters);
            _filters = filters;
        }

        /// <summary>
        /// Returns the current subscription state.
        /// </summary>
        public TsCDaSubscriptionState GetState()
        {
            _state = _subscription.GetState();
            return _state;
        }

        /// <summary>
        /// Updates the current subscription state.
        /// </summary>
        public TsCDaSubscriptionState ModifyState(int masks, TsCDaSubscriptionState state)
        {
            _state = _subscription.ModifyState(masks, state);
            return _state;
        }

        /// <summary>
        /// Adds items to the subscription.
        /// </summary>
        public virtual TsCDaItemResult[] AddItems(TsCDaItem[] items)
        {
            if (items == null) throw new ArgumentNullException("items");

            // check if there is nothing to do.
            if (items.Length == 0)
            {
                return new TsCDaItemResult[0];
            }

            // add items.
            TsCDaItemResult[] results = _subscription.AddItems(items);

            if (results == null || results.Length == 0)
            {
                throw new OpcResultException(new OpcResult((int)OpcResult.E_FAIL.Code, OpcResult.FuncCallType.SysFuncCall, null), "The browse operation cannot continue");
            }

            // update locale item list.
            ArrayList itemList = new ArrayList();
            if (_items != null) itemList.AddRange(_items);

            for (int ii = 0; ii < results.Length; ii++)
            {
                // check for failure.
                if (results[ii].Result.Failed())
                {
                    continue;
                }

                // create locale copy of the item.
                // item name, item path and client handle may not be returned by server.
                TsCDaItem item = new TsCDaItem(results[ii]) { ItemName = items[ii].ItemName, ItemPath = items[ii].ItemPath, ClientHandle = items[ii].ClientHandle };

                itemList.Add(item);
            }

            // save the new item list.
            _items = (TsCDaItem[])itemList.ToArray(typeof(TsCDaItem));

            // update the local state.
            GetState();

            // return results.
            return results;
        }

        /// <summary>
        /// Modifies items that are already part of the subscription.
        /// </summary>
        public virtual TsCDaItemResult[] ModifyItems(int masks, TsCDaItem[] items)
        {
            if (items == null) throw new ArgumentNullException("items");

            // check if there is nothing to do.
            if (items.Length == 0)
            {
                return new TsCDaItemResult[0];
            }

            // modify items.
            TsCDaItemResult[] results = _subscription.ModifyItems(masks, items);

            if (results == null || results.Length == 0)
            {
                throw new OpcResultException(new OpcResult((int)OpcResult.E_FAIL.Code, OpcResult.FuncCallType.SysFuncCall, null), "The browse operation cannot continue");
            }

            // update local item - modify item success means all fields were updated successfully.
            for (int ii = 0; ii < results.Length; ii++)
            {
                // check for failure.
                if (results[ii].Result.Failed())
                {
                    continue;
                }

                // search local item list.
                for (int jj = 0; jj < _items.Length; jj++)
                {
                    if (_items[jj].ServerHandle.Equals(items[ii].ServerHandle))
                    {
                        // update locale copy of the item.
                        // item name, item path and client handle may not be returned by server.
                        TsCDaItem item = new TsCDaItem(results[ii]) { ItemName = _items[jj].ItemName, ItemPath = _items[jj].ItemPath, ClientHandle = _items[jj].ClientHandle };

                        _items[jj] = item;
                        break;
                    }
                }
            }

            // update the local state.
            GetState();

            // return results.
            return results;
        }

        /// <summary>
        /// Removes items from a subsription.
        /// </summary>
        public virtual OpcItemResult[] RemoveItems(OpcItem[] items)
        {
            if (items == null) throw new ArgumentNullException("items");

            // check if there is nothing to do.
            if (items.Length == 0)
            {
                return new OpcItemResult[0];
            }

            // remove items from server.
            OpcItemResult[] results = _subscription.RemoveItems(items);

            if (results == null || results.Length == 0)
            {
                throw new OpcResultException(new OpcResult((int)OpcResult.E_FAIL.Code, OpcResult.FuncCallType.SysFuncCall, null), "The browse operation cannot continue");
            }

            // remove items from local list if successful.
            ArrayList itemList = new ArrayList();

            foreach (TsCDaItem item in _items)
            {
                bool removed = false;

                for (int ii = 0; ii < results.Length; ii++)
                {
                    if (item.ServerHandle.Equals(items[ii].ServerHandle))
                    {
                        removed = results[ii].Result.Succeeded();
                        break;
                    }
                }

                if (!removed) itemList.Add(item);
            }

            // update local list.
            _items = (TsCDaItem[])itemList.ToArray(typeof(TsCDaItem));

            // update the local state.
            GetState();

            // return results.
            return results;
        }

        /// <summary>
        /// Reads a set of subscription items.
        /// </summary>
        public TsCDaItemValueResult[] Read(TsCDaItem[] items)
        {
            return _subscription.Read(items);
        }

        /// <summary>
        /// Writes a set of subscription items.
        /// </summary>
        public OpcItemResult[] Write(TsCDaItemValue[] items)
        {
            return _subscription.Write(items);
        }

        /// <summary>
        /// Begins an asynchronous read operation for a set of items.
        /// </summary>
        /// <param name="items">The set of items to read (must include the item name).</param>
        /// <param name="requestHandle">An identifier for the request assigned by the caller.</param>
        /// <param name="callback">A delegate used to receive notifications when the request completes.</param>
        /// <param name="request">An object that contains the state of the request (used to cancel the request).</param>
        /// <returns>A set of results containing any errors encountered when the server validated the items.</returns>
        public OpcItemResult[] Read(
            TsCDaItem[] items,
            object requestHandle,
            TsCDaReadCompleteEventHandler callback,
            out IOpcRequest request)
        {
            return _subscription.Read(items, requestHandle, callback, out request);
        }

        /// <summary>
        /// Begins an asynchronous write operation for a set of items.
        /// </summary>
        /// <param name="items">The set of item values to write (must include the item name).</param>
        /// <param name="requestHandle">An identifier for the request assigned by the caller.</param>
        /// <param name="callback">A delegate used to receive notifications when the request completes.</param>
        /// <param name="request">An object that contains the state of the request (used to cancel the request).</param>
        /// <returns>A set of results containing any errors encountered when the server validated the items.</returns>
        public OpcItemResult[] Write(
            TsCDaItemValue[] items,
            object requestHandle,
            TsCDaWriteCompleteEventHandler callback,
            out IOpcRequest request)
        {
            return _subscription.Write(items, requestHandle, callback, out request);
        }

        /// <summary>
        /// Cancels an asynchronous request.
        /// </summary>
        public void Cancel(IOpcRequest request, TsCDaCancelCompleteEventHandler callback)
        {
            _subscription.Cancel(request, callback);
        }

        /// <summary>
        /// Tells the server to send an data change update for all subscription items. 
        /// </summary>
        public void Refresh() { _subscription.Refresh(); }

        /// <summary>
        /// Causes the server to send a data changed notification for all active items. 
        /// </summary>
        /// <param name="requestHandle">An identifier for the request assigned by the caller.</param>
        /// <param name="request">An object that contains the state of the request (used to cancel the request).</param>
        /// <returns>A set of results containing any errors encountered when the server validated the items.</returns>
        public void Refresh(
            object requestHandle,
            out IOpcRequest request)
        {
            _subscription.Refresh(requestHandle, out request);
        }

        /// <summary>
        /// Sets whether data change callbacks are enabled.
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            _subscription.SetEnabled(enabled);
            _enabled = enabled;
        }

        /// <summary>
        /// Gets whether data change callbacks are enabled.
        /// </summary>
        public bool GetEnabled()
        {
            _enabled = _subscription.GetEnabled();
            return _enabled;
        }

        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region ISubscription

        /// <summary>
        /// An event to receive data change updates.
        /// </summary>
        public event TsCDaDataChangedEventHandler DataChangedEvent {
            add { _subscription.DataChangedEvent += value; }
            remove { _subscription.DataChangedEvent -= value; }
        }

        #endregion
    }
}
