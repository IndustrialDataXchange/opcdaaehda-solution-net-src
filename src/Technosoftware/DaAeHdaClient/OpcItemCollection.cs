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
using System.Text;
using System.Xml;
#endregion

namespace Technosoftware.DaAeHdaClient
{
    /// <summary>
    /// A collection of item identifiers.
    /// </summary>
    [Serializable]
    public class OpcItemCollection : ICloneable, ICollection
    {
        /// <summary>
        /// Creates an empty collection.
        /// </summary>
        public OpcItemCollection()
        {
            // do nothing.
        }

        /// <summary>
        /// Initializes the object with any ItemIdentifiers contained in the collection.
        /// </summary>
        /// <param name="collection">A collection containing item ids.</param>
        public OpcItemCollection(ICollection collection)
        {
            Init(collection);
        }

        /// <summary>
        /// Returns the itemID at the specified index.
        /// </summary>
        public OpcItem this[int index]
        {
            get { return this.itemIDs[index]; }
            set { this.itemIDs[index] = value; }
        }

        /// <summary>
        /// Initializes the object with any item ids contained in the collection.
        /// </summary>
        /// <param name="collection">A collection containing item ids.</param>
        public void Init(ICollection collection)
        {
            Clear();

            if (collection != null)
            {
                ArrayList itemIDs = new ArrayList(collection.Count);

                foreach (object value in collection)
                {
                    if (typeof(OpcItem).IsInstanceOfType(value))
                    {
                        itemIDs.Add(((OpcItem)value).Clone());
                    }
                }

                this.itemIDs = (OpcItem[])itemIDs.ToArray(typeof(OpcItem));
            }
        }

        /// <summary>
        /// Removes all itemIDs in the collection.
        /// </summary>
        public void Clear()
        {
            this.itemIDs = new OpcItem[0];
        }

        #region ICloneable Members
        /// <summary>
        /// Creates a deep copy of the object.
        /// </summary>
        public virtual object Clone()
        {
            return new OpcItemCollection(this);
        }
        #endregion

        #region ICollection Members
        /// <summary>
        /// Indicates whether access to the ICollection is synchronized (thread-safe).
        /// </summary>
        public bool IsSynchronized
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the number of objects in the collection.
        /// </summary>
        public int Count
        {
            get { return (this.itemIDs != null) ? this.itemIDs.Length : 0; }
        }

        /// <summary>
        /// Copies the objects to an Array, starting at a the specified index.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination for the objects.</param>
        /// <param name="index">The zero-based index in the Array at which copying begins.</param>
        public void CopyTo(Array array, int index)
        {
            if (this.itemIDs != null)
            {
                this.itemIDs.CopyTo(array, index);
            }
        }

        /// <summary>
        /// Copies the objects to an Array, starting at a the specified index.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination for the objects.</param>
        /// <param name="index">The zero-based index in the Array at which copying begins.</param>
        public void CopyTo(OpcItem[] array, int index)
        {
            CopyTo((Array)array, index);
        }

        /// <summary>
        /// Indicates whether access to the ICollection is synchronized (thread-safe).
        /// </summary>
        public object SyncRoot
        {
            get { return this; }
        }
        #endregion

        #region IEnumerable Members
        /// <summary>
        /// Returns an enumerator that can iterate through a collection.
        /// </summary>
        /// <returns>An IEnumerator that can be used to iterate through the collection.</returns>
        public IEnumerator GetEnumerator()
        {
            return this.itemIDs.GetEnumerator();
        }
        #endregion

        #region Private Members
        private OpcItem[] itemIDs = new OpcItem[0];
        #endregion
    }
}
