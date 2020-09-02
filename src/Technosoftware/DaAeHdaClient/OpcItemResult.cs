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
#endregion

namespace Technosoftware.DaAeHdaClient
{
    /// <summary>
    /// A result code associated with a unique item identifier.
    /// </summary>
    [Serializable]
    public class OpcItemResult : OpcItem, IOpcResult
    {
        ///////////////////////////////////////////////////////////////////////
        #region Fields

        private OpcResult _result = OpcResult.S_OK;
        private string _diagnosticInfo = null;

        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region Constructors, Destructor, Initialization
        
        /// <summary>
        /// Initialize object with default values.
        /// </summary>
        public OpcItemResult() { }

        /// <summary>
        /// Initialize object with the specified OpcItem object.
        /// </summary>
        public OpcItemResult(OpcItem item)
            : base(item)
        {
        }

        /// <summary>
        /// Initialize object with the specified IdentifiedResult object.
        /// </summary>
        public OpcItemResult(OpcItemResult item)
            : base(item)
        {
            if (item != null)
            {
                Result = item.Result;
                DiagnosticInfo = item.DiagnosticInfo;
            }
        }

        /// <summary>
        /// Initializes the object with the specified item name and result code.
        /// </summary>
        public OpcItemResult(string itemName, OpcResult resultID)
            : base(itemName)
        {
            Result = resultID;
        }

        /// <summary>
        /// Initialize object with the specified item name, result code and diagnostic info.
        /// </summary>
        public OpcItemResult(string itemName, OpcResult resultID, string diagnosticInfo)
            : base(itemName)
        {
            Result = resultID;
            DiagnosticInfo = diagnosticInfo;
        }

        /// <summary>
        /// Initialize object with the specified OpcItem and result code.
        /// </summary>
        public OpcItemResult(OpcItem item, OpcResult resultID)
            : base(item)
        {
            Result = resultID;
        }

        /// <summary>
        /// Initialize object with the specified OpcItem, result code and diagnostic info.
        /// </summary>
        public OpcItemResult(OpcItem item, OpcResult resultID, string diagnosticInfo)
            : base(item)
        {
            Result = resultID;
            DiagnosticInfo = diagnosticInfo;
        }

        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region IOpcResult Members

        /// <summary>
        /// The error id for the result of an operation on an item.
        /// </summary>
        public OpcResult Result
        {
            get { return _result; }
            set { _result = value; }
        }

        /// <summary>
        /// Vendor specific diagnostic information (not the localized error text).
        /// </summary>
        public string DiagnosticInfo
        {
            get { return _diagnosticInfo; }
            set { _diagnosticInfo = value; }
        }

        #endregion

    }
}
