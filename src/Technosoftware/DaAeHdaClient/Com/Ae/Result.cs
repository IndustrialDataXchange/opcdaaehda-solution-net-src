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
using System.Runtime.InteropServices;
#endregion

namespace Technosoftware.DaAeHdaClient.Com.Ae
{
    /// <summary>
    /// Defines all well known COM AE HRESULT codes.
    /// </summary>
    internal struct Result
    {       
        /// <remarks/>
        public const int S_ALREADYACKED         = +0x00040200; // 0x00040200
        /// <remarks/>
        public const int S_INVALIDBUFFERTIME    = +0x00040201; // 0x00040201
        /// <remarks/>
        public const int S_INVALIDMAXSIZE       = +0x00040202; // 0x00040202
        /// <remarks/>
        public const int S_INVALIDKEEPALIVETIME = +0x00040203; // 0x00040203
        /// <remarks/>
        public const int E_INVALIDBRANCHNAME    = -0x3FFBFDFD; // 0xC0040203
        /// <remarks/>
        public const int E_INVALIDTIME          = -0x3FFBFDFC; // 0xC0040204
        /// <remarks/>
        public const int E_BUSY                 = -0x3FFBFDFB; // 0xC0040205
        /// <remarks/>
        public const int E_NOINFO               = -0x3FFBFDFA; // 0xC0040206
    }
}
