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

namespace Technosoftware.DaAeHdaClient.Da
{
	/// <summary>
	/// Defines a set of filters to apply when browsing.
	/// </summary>
	[Serializable]
	public class TsCDaBrowseFilters : ICloneable
	{
		///////////////////////////////////////////////////////////////////////
		#region Fields

		private TsCDaBrowseFilter _browseFilter = TsCDaBrowseFilter.All;
		private TsDaPropertyID[] _propertyIDs = null;

		#endregion

		///////////////////////////////////////////////////////////////////////
		#region Properties

		/// <summary>
		/// The maximum number of elements to return. Zero means no limit.
		/// </summary>
		public int MaxElementsReturned { get; set; }

		/// <summary>
		/// The type of element to return.
		/// </summary>
		public TsCDaBrowseFilter BrowseFilter
		{
			get { return _browseFilter; }
			set { _browseFilter = value; }
		}

		/// <summary>
		/// An expression used to match the name of the element.
		/// </summary>
		public string ElementNameFilter { get; set; }

		/// <summary>
		/// A filter which has semantics that defined by the server.
		/// </summary>
		public string VendorFilter { get; set; }

		/// <summary>
		/// Whether all supported properties to return with each element.
		/// </summary>
		public bool ReturnAllProperties { get; set; }

		/// <summary>
		/// A list of names of the properties to return with each element.
		/// </summary>
		public TsDaPropertyID[] PropertyIDs
		{
			get { return _propertyIDs; }
			set { _propertyIDs = value; }
		}

		/// <summary>
		/// Whether property values should be returned with the properties.
		/// </summary>
		public bool ReturnPropertyValues { get; set; }

		#endregion

		///////////////////////////////////////////////////////////////////////
		#region ICloneable Members

		/// <summary>
		/// Creates a deep copy of the object.
		/// </summary>
		public virtual object Clone()
		{
			TsCDaBrowseFilters clone = (TsCDaBrowseFilters)MemberwiseClone();
			clone.PropertyIDs = (TsDaPropertyID[])((PropertyIDs != null) ? PropertyIDs.Clone() : null);
			return clone;
		}

		#endregion
	}
}
