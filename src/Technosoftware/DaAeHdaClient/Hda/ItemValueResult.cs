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

namespace Technosoftware.DaAeHdaClient.Hda
{
	/// <summary>
	/// A result associated with a single item value.
	/// </summary>
	[Serializable]
	public class TsCHdaResult : ICloneable, IOpcResult
	{
		///////////////////////////////////////////////////////////////////////
		#region Fields

		private OpcResult _result = OpcResult.S_OK;

		#endregion

		///////////////////////////////////////////////////////////////////////
		#region Constructors, Destructor, Initialization

		/// <summary>
		/// Initializes the object with default values.
		/// </summary>
		public TsCHdaResult() { }

		/// <summary>
		/// Initializes the object with the specified result id.
		/// </summary>
		public TsCHdaResult(OpcResult resultID)
		{
			Result = resultID;
			DiagnosticInfo = null;
		}

		/// <summary>
		/// Initializes the object with the specified result object.
		/// </summary>
		public TsCHdaResult(IOpcResult result)
		{
			Result = result.Result;
			DiagnosticInfo = result.DiagnosticInfo;
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
		public string DiagnosticInfo { get; set; }
	
		#endregion

		///////////////////////////////////////////////////////////////////////
		#region ICloneable Members

		/// <summary>
		/// Creates a deep copy of the object.
		/// </summary>
		public object Clone()
		{
			return MemberwiseClone();
		}

		#endregion
	}
}
