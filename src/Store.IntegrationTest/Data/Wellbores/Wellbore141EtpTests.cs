﻿//----------------------------------------------------------------------- 
// PDS WITSMLstudio Store, 2017.2
//
// Copyright 2017 PDS Americas LLC
// 
// Licensed under the PDS Open Source WITSML Product License Agreement (the
// "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   
//     http://www.pds.group/WITSMLstudio/OpenSource/ProductLicenseAgreement
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-----------------------------------------------------------------------

using System.Threading.Tasks;
using Energistics;
using Energistics.Common;
using Energistics.DataAccess;
using Energistics.DataAccess.WITSML141;
using Energistics.DataAccess.WITSML141.ReferenceData;
using Energistics.Protocol.Store;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PDS.WITSMLstudio.Store.Data.Wellbores
{
    /// <summary>
    /// Wellbore141EtpTests
    /// </summary>
    public partial class Wellbore141EtpTests
    {
        [TestMethod]
        public async Task Wellbore141_DeleteObject_Cannot_Delete_Well_With_Child_Object()
        {
            AddParents();
            await RequestSessionAndAssert();

            var handler = _client.Handler<IStoreCustomer>();
            var uri = Wellbore.GetUri();

            var dataObject = CreateDataObject<WellboreList, Wellbore>(uri, Wellbore);

            // Put Object
            await PutAndAssert(handler, dataObject);

            var message = new Message()
            {
                UidWell = Well.Uid,
                UidWellbore = Wellbore.Uid,
                Uid = DevKit.Uid(),
                NameWell = Well.Name,
                NameWellbore = Wellbore.Name,
                Name = DevKit.Name("Wellbore"),
                DateTime = new Timestamp(),
                TypeMessage = MessageType.unknown
            };

            var messageObject = CreateDataObject<MessageList, Message>(message.GetUri(), message);

            // Put Message
            await PutAndAssert(handler, messageObject);

            // Delete Wellbore
            await DeleteAndAssert(handler, uri, EtpErrorCodes.NoCascadeDelete);

            // Delete Message
            await DeleteAndAssert(handler, message.GetUri());

            // Delete Wellbore
            await DeleteAndAssert(handler, uri);
        }
    }
}
