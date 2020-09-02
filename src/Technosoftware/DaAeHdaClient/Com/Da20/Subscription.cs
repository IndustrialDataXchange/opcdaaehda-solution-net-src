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
using Technosoftware.DaAeHdaClient;
using Technosoftware.DaAeHdaClient.Da;
using Technosoftware.DaAeHdaClient.Com;
using Technosoftware.DaAeHdaClient.Com.Da;
using OpcRcw.Da;
using OpcRcw.Comn;
#endregion

namespace Technosoftware.DaAeHdaClient.Com.Da20
{
    /// <summary>
    /// An in-process wrapper for a remote OPC Data Access 2.0X subscription.
    /// </summary>
	internal class Subscription : Technosoftware.DaAeHdaClient.Com.Da.Subscription
    {

        //======================================================================
        // Construction

        /// <summary>
        /// Initializes a new instance of a subscription.
        /// </summary>
        internal Subscription(object subscription, TsCDaSubscriptionState state, int filters)
            :
            base(subscription, state, filters)
        {
        }

		//======================================================================
		// State Management

		/// <summary>
		/// Returns the current state of the subscription.
		/// </summary>
		/// <returns>The current state of the subscription.</returns>
		public TsCDaSubscriptionState GetState() 
		{
			lock (this)
			{ 
				TsCDaSubscriptionState state = new TsCDaSubscriptionState();

				state.ClientHandle = _handle;

                string methodName = "IOPCGroupStateMgt.GetState";
				try
                {
					string name         = null;
					int    active       = 0;
					int    updateRate   = 0;
					float  deadband     = 0;
					int    timebias     = 0;
					int    localeID     = 0;
					int    clientHandle = 0;
					int    serverHandle = 0;

                    IOPCGroupStateMgt subscription = BeginComCall<IOPCGroupStateMgt>(methodName, true);
                    subscription.GetState(
						out updateRate,
						out active,
						out name,
						out timebias,
						out deadband,
						out localeID,
						out clientHandle,
						out serverHandle);

					state.Name         = name;
					state.ServerHandle = serverHandle;
                    if (active == 1)
                    {
                        state.Active = true;
                    }
                    else
                    {
                        state.Active = false;
                    }
					state.UpdateRate   = updateRate;
					state.TimeBias     = timebias;
					state.Deadband     = deadband;
					state.Locale       = Technosoftware.DaAeHdaClient.Utilities.Interop.GetLocale(localeID);

					// cache the name separately.
					name_ = state.Name;

					state.KeepAlive = 0;
				}
				catch (Exception e)
				{
                    ComCallError(methodName, e);
					throw Technosoftware.DaAeHdaClient.Utilities.Interop.CreateException(methodName, e);
				}
                finally
                {
                    EndComCall(methodName);
                }

                return state;
			}
		}

        //======================================================================
        // ISubscription

        /// <summary>
        /// Tells the server to send an data change update for all subscription items containing the cached values. 
        /// </summary>
        public override void Refresh()
        {
            lock (this)
            {
                if (subscription_ == null) throw new NotConnectedException();

                string methodName = "IOPCAsyncIO2.Refresh2";
                try
                {
                    int cancelID = 0;
                    IOPCAsyncIO2 subscription = BeginComCall<IOPCAsyncIO2>(methodName, true);
                    subscription.Refresh2(OPCDATASOURCE.OPC_DS_CACHE, ++_counter, out cancelID);
                }
                catch (Exception e)
                {
                    ComCallError(methodName, e);
                    throw Technosoftware.DaAeHdaClient.Utilities.Interop.CreateException(methodName, e);
                }
                finally
                {
                    EndComCall(methodName);
                }
            }
        }

        /// <summary>
        /// Sets whether data change callbacks are enabled.
        /// </summary>
        public override void SetEnabled(bool enabled)
        {
            lock (this)
            {
                if (subscription_ == null) throw new NotConnectedException();

                string methodName = "IOPCAsyncIO2.SetEnable";
                try
                {
                    IOPCAsyncIO2 subscription = BeginComCall<IOPCAsyncIO2>(methodName, true);
                    subscription.SetEnable((enabled)?1:0);
                }
                catch (Exception e)
                {
                    ComCallError(methodName, e);
                    throw Technosoftware.DaAeHdaClient.Utilities.Interop.CreateException(methodName, e);
                }
                finally
                {
                    EndComCall(methodName);
                }
            }
        }

        /// <summary>
        /// Gets whether data change callbacks are enabled.
        /// </summary>
        public override bool GetEnabled()
        {
            lock (this)
            {
                if (subscription_ == null) throw new NotConnectedException();

                string methodName = "IOPCAsyncIO2.GetEnable";
                try
                {
					int enabled = 0;
                    IOPCAsyncIO2 subscription = BeginComCall<IOPCAsyncIO2>(methodName, true);
                    subscription.GetEnable(out enabled);
					return enabled != 0;
                }
                catch (Exception e)
                {
                    ComCallError(methodName, e);
                    throw Technosoftware.DaAeHdaClient.Utilities.Interop.CreateException(methodName, e);
                }
                finally
                {
                    EndComCall(methodName);
                }
            }
        }

        //======================================================================
        // Private Methods

        /// <summary>
        /// Reads a set of items using DA2.0 interfaces.
        /// </summary>
        protected override TsCDaItemValueResult[] Read(OpcItem[] itemIDs, TsCDaItem[] items)
        {
            // create result list.
            TsCDaItemValueResult[] results = new TsCDaItemValueResult[itemIDs.Length];

            // separate into cache reads and device reads.
            ArrayList cacheReads = new ArrayList();
            ArrayList deviceReads = new ArrayList();

            for (int ii = 0; ii < itemIDs.Length; ii++)
            {
                results[ii] = new TsCDaItemValueResult(itemIDs[ii]);

				if (items[ii].MaxAgeSpecified && (items[ii].MaxAge < 0 || items[ii].MaxAge == Int32.MaxValue))
                    {
                        cacheReads.Add(results[ii]);
                    }
                    else
                    {
                        deviceReads.Add(results[ii]);
                    }
                }

            // read items from cache.
            if (cacheReads.Count > 0)
            {
                Read((TsCDaItemValueResult[])cacheReads.ToArray(typeof(TsCDaItemValueResult)), true);
            }

            // read items from device.
            if (deviceReads.Count > 0)
            {
                Read((TsCDaItemValueResult[])deviceReads.ToArray(typeof(TsCDaItemValueResult)), false);
            }

            // return results.
            return results;
        }

        /// <summary>
        /// Reads a set of values.
        /// </summary>
        private void Read(TsCDaItemValueResult[] items, bool cache)
        {
            if (items.Length == 0) return;

            // marshal input parameters.
            int[] serverHandles = new int[items.Length];

            for (int ii = 0; ii < items.Length; ii++)
            {
                serverHandles[ii] = (int)items[ii].ServerHandle;
            }

            // initialize output parameters.
            IntPtr pValues = IntPtr.Zero;
            IntPtr pErrors = IntPtr.Zero;

            string methodName = "IOPCSyncIO.Read";
            try
            {
                IOPCSyncIO subscription = BeginComCall<IOPCSyncIO>(methodName, true);
                subscription.Read(
                    (cache) ? OPCDATASOURCE.OPC_DS_CACHE : OPCDATASOURCE.OPC_DS_DEVICE,
                    items.Length,
                    serverHandles,
                    out pValues,
                    out pErrors);
            }
            catch (Exception e)
            {
                ComCallError(methodName, e);
                throw Technosoftware.DaAeHdaClient.Utilities.Interop.CreateException(methodName, e);
            }
            finally
            {
                EndComCall(methodName);
            }

            // unmarshal output parameters.
            TsCDaItemValue[] values = Technosoftware.DaAeHdaClient.Com.Da.Interop.GetItemValues(ref pValues, items.Length, true);
            int[] errors = Technosoftware.DaAeHdaClient.Utilities.Interop.GetInt32s(ref pErrors, items.Length, true);

            // construct results list.
            for (int ii = 0; ii < items.Length; ii++)
            {
                items[ii].Result = Technosoftware.DaAeHdaClient.Utilities.Interop.GetResultID(errors[ii]);
                items[ii].DiagnosticInfo = null;

                // convert COM code to unified DA code.
                if (errors[ii] == Result.E_BADRIGHTS) { items[ii].Result = new OpcResult(OpcResult.Da.E_WRITEONLY, Result.E_BADRIGHTS); }

                if (items[ii].Result.Succeeded())
                {
                    items[ii].Value = values[ii].Value;
                    items[ii].Quality = values[ii].Quality;
                    items[ii].QualitySpecified = values[ii].QualitySpecified;
                    items[ii].Timestamp = values[ii].Timestamp;
                    items[ii].TimestampSpecified = values[ii].TimestampSpecified;
                }
            }
        }

        /// <summary>
        /// Writes a set of items using DA2.0 interfaces.
        /// </summary>
        protected override OpcItemResult[] Write(OpcItem[] itemIDs, TsCDaItemValue[] items)
        {
            // create result list.
            OpcItemResult[] results = new OpcItemResult[itemIDs.Length];

            // construct list of valid items to write.
            ArrayList writeItems = new ArrayList(itemIDs.Length);
            ArrayList writeValues = new ArrayList(itemIDs.Length);

            for (int ii = 0; ii < items.Length; ii++)
            {
                results[ii] = new OpcItemResult(itemIDs[ii]);

                if (items[ii].QualitySpecified || items[ii].TimestampSpecified)
                {
                    results[ii].Result = OpcResult.Da.E_NO_WRITEQT;
                    results[ii].DiagnosticInfo = null;
                    continue;
                }

                writeItems.Add(results[ii]);
                writeValues.Add(items[ii]);
            }

            // check if there is nothing to do.
            if (writeItems.Count == 0)
            {
                return results;
            }

            // initialize input parameters.
            int[] serverHandles = new int[writeItems.Count];
            object[] values = new object[writeItems.Count];

            for (int ii = 0; ii < serverHandles.Length; ii++)
            {
                serverHandles[ii] = (int)((OpcItemResult)writeItems[ii]).ServerHandle;
                values[ii] = Technosoftware.DaAeHdaClient.Utilities.Interop.GetVARIANT(((TsCDaItemValue)writeValues[ii]).Value);
            }

            IntPtr pErrors = IntPtr.Zero;

            // write item values.
            string methodName = "IOPCSyncIO.Write";
            try
            {
                IOPCSyncIO subscription = BeginComCall<IOPCSyncIO>(methodName, true);
                subscription.Write(
                    writeItems.Count,
                    serverHandles,
                    values,
                    out pErrors);
            }
            catch (Exception e)
            {
                ComCallError(methodName, e);
                throw Technosoftware.DaAeHdaClient.Utilities.Interop.CreateException(methodName, e);
            }
            finally
            {
                EndComCall(methodName);
            }

            // unmarshal results.
            int[] errors = Technosoftware.DaAeHdaClient.Utilities.Interop.GetInt32s(ref pErrors, writeItems.Count, true);

            for (int ii = 0; ii < writeItems.Count; ii++)
            {
                OpcItemResult result = (OpcItemResult)writeItems[ii];

                result.Result = Technosoftware.DaAeHdaClient.Utilities.Interop.GetResultID(errors[ii]);
                result.DiagnosticInfo = null;

                // convert COM code to unified DA code.
                if (errors[ii] == Result.E_BADRIGHTS) { results[ii].Result = new OpcResult(OpcResult.Da.E_READONLY, Result.E_BADRIGHTS); }
            }

            // return results.
            return results;
        }

        /// <summary>
        /// Begins an asynchronous read of a set of items using DA2.0 interfaces.
        /// </summary>
        protected override OpcItemResult[] BeginRead(
            OpcItem[] itemIDs,
            TsCDaItem[] items,
            int requestID,
            out int cancelID)
        {
            string methodName = "IOPCAsyncIO2.Read";
            try
            {
                // marshal input parameters.
                int[] serverHandles = new int[itemIDs.Length];

                for (int ii = 0; ii < itemIDs.Length; ii++)
                {
                    serverHandles[ii] = (int)itemIDs[ii].ServerHandle;
                }

                // initialize output parameters.
                IntPtr pErrors = IntPtr.Zero;

                IOPCAsyncIO2 subscription = BeginComCall<IOPCAsyncIO2>(methodName, true);
                subscription.Read(
                    itemIDs.Length,
                    serverHandles,
                    requestID,
                    out cancelID,
                    out pErrors);

                // unmarshal output parameters.
                int[] errors = Technosoftware.DaAeHdaClient.Utilities.Interop.GetInt32s(ref pErrors, itemIDs.Length, true);

                // create item results.
                OpcItemResult[] results = new OpcItemResult[itemIDs.Length];

                for (int ii = 0; ii < itemIDs.Length; ii++)
                {
                    results[ii] = new OpcItemResult(itemIDs[ii]);
                    results[ii].Result = Technosoftware.DaAeHdaClient.Utilities.Interop.GetResultID(errors[ii]);
                    results[ii].DiagnosticInfo = null;

                    // convert COM code to unified DA code.
                    if (errors[ii] == Result.E_BADRIGHTS) { results[ii].Result = new OpcResult(OpcResult.Da.E_WRITEONLY, Result.E_BADRIGHTS); }
                }

                // return results.
                return results;
            }
            catch (Exception e)
            {
                ComCallError(methodName, e);
                throw Technosoftware.DaAeHdaClient.Utilities.Interop.CreateException(methodName, e);
            }
            finally
            {
                EndComCall(methodName);
            }
        }

        /// <summary>
        /// Begins an asynchronous write for a set of items using DA2.0 interfaces.
        /// </summary>
        protected override OpcItemResult[] BeginWrite(
            OpcItem[] itemIDs,
            TsCDaItemValue[] items,
            int requestID,
            out int cancelID)
        {
            cancelID = 0;

            ArrayList validItems = new ArrayList();
            ArrayList validValues = new ArrayList();

            // construct initial result list.
            OpcItemResult[] results = new OpcItemResult[itemIDs.Length];

            for (int ii = 0; ii < itemIDs.Length; ii++)
            {
                results[ii] = new OpcItemResult(itemIDs[ii]);

                results[ii].Result = OpcResult.S_OK;
                results[ii].DiagnosticInfo = null;

                if (items[ii].QualitySpecified || items[ii].TimestampSpecified)
                {
                    results[ii].Result = OpcResult.Da.E_NO_WRITEQT;
                    results[ii].DiagnosticInfo = null;
                    continue;
                }

                validItems.Add(results[ii]);
                validValues.Add(Technosoftware.DaAeHdaClient.Utilities.Interop.GetVARIANT(items[ii].Value));
            }

            // check if any valid items exist.
            if (validItems.Count == 0)
            {
                return results;
            }

            string methodName = "IOPCAsyncIO2.Write";
            try
            {
                // initialize input parameters.
                int[] serverHandles = new int[validItems.Count];

                for (int ii = 0; ii < validItems.Count; ii++)
                {
                    serverHandles[ii] = (int)((OpcItemResult)validItems[ii]).ServerHandle;
                }

                // write to sever.
                IntPtr pErrors = IntPtr.Zero;

                IOPCAsyncIO2 subscription = BeginComCall<IOPCAsyncIO2>(methodName, true);
                subscription.Write(
                    validItems.Count,
                    serverHandles,
                    (object[])validValues.ToArray(typeof(object)),
                    requestID,
                    out cancelID,
                    out pErrors);

                // unmarshal results.
                int[] errors = Technosoftware.DaAeHdaClient.Utilities.Interop.GetInt32s(ref pErrors, validItems.Count, true);

                // create result list.
                for (int ii = 0; ii < validItems.Count; ii++)
                {
                    OpcItemResult result = (OpcItemResult)validItems[ii];

                    result.Result = Technosoftware.DaAeHdaClient.Utilities.Interop.GetResultID(errors[ii]);
                    result.DiagnosticInfo = null;

                    // convert COM code to unified DA code.
                    if (errors[ii] == Result.E_BADRIGHTS) { results[ii].Result = new OpcResult(OpcResult.Da.E_READONLY, Result.E_BADRIGHTS); }
                }
            }
            catch (Exception e)
            {
                ComCallError(methodName, e);
                throw Technosoftware.DaAeHdaClient.Utilities.Interop.CreateException(methodName, e);
            }
            finally
            {
                EndComCall(methodName);
            }

            // return results.
            return results;
        }
    }
}
