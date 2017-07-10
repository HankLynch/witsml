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

// ----------------------------------------------------------------------
// <auto-generated>
//     Changes to this file may cause incorrect behavior and will be lost
//     if the code is regenerated.
// </auto-generated>
// ----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Energistics.Common;
using Energistics.DataAccess;
using Energistics.DataAccess.WITSML200;
using Energistics.DataAccess.WITSML200.ComponentSchemas;
using Energistics.DataAccess.WITSML200.ReferenceData;
using Energistics.Datatypes;
using Energistics.Protocol;
using Energistics.Protocol.Core;
using Energistics.Protocol.Discovery;
using Energistics.Protocol.Store;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PDS.WITSMLstudio.Store.Data.ChannelSets
{
    [TestClass]
    public partial class ChannelSet200EtpTests : ChannelSet200TestBase
    {
        partial void BeforeEachTest();

        partial void AfterEachTest();

        protected override void OnTestSetUp()
        {
            EtpSetUp(DevKit.Container);
            BeforeEachTest();
            _server.Start();
        }

        protected override void OnTestCleanUp()
        {
            _server?.Stop();
            EtpCleanUp();
            AfterEachTest();
        }

        [TestMethod]
        public void ChannelSet200_Ensure_Creates_ChannelSet_With_Default_Values()
        {
            DevKit.EnsureAndAssert(ChannelSet);
        }

        [TestMethod]
        public async Task ChannelSet200_GetResources_Can_Get_All_ChannelSet_Resources()
        {
            AddParents();
            DevKit.AddAndAssert(ChannelSet);
            await RequestSessionAndAssert();

            var uri = ChannelSet.GetUri();
            var parentUri = uri.Parent;

            await GetResourcesAndAssert(ChannelSet.Wellbore.GetUri());

            var folderUri = parentUri.Append(uri.ObjectType);
            await GetResourcesAndAssert(folderUri);
        }

        [TestMethod]
        public async Task ChannelSet200_PutObject_Can_Add_ChannelSet()
        {
            AddParents();
            await RequestSessionAndAssert();

            var handler = _client.Handler<IStoreCustomer>();
            var uri = ChannelSet.GetUri();

            var dataObject = CreateDataObject(uri, ChannelSet);

            // Get Object Expecting it Not to Exist
            await GetAndAssert(handler, uri, Energistics.EtpErrorCodes.NotFound);

            // Put Object
            await PutAndAssert(handler, dataObject);

            // Get Object
            var args = await GetAndAssert(handler, uri);

            // Check Data Object XML
            Assert.IsNotNull(args?.Message.DataObject);
            var xml = args.Message.DataObject.GetString();

            var result = Parse<ChannelSet>(xml);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task ChannelSet200_PutObject_Can_Update_ChannelSet()
        {
            AddParents();
            await RequestSessionAndAssert();

            var handler = _client.Handler<IStoreCustomer>();
            var uri = ChannelSet.GetUri();

            // Add a ExtensionNameValue to Data Object
            var envName = "TestPutObject";
            var env = DevKit.ExtensionNameValue(envName, envName);
            ChannelSet.ExtensionNameValue = new List<ExtensionNameValue>() {env};

            var dataObject = CreateDataObject(uri, ChannelSet);

            // Get Object Expecting it Not to Exist
            await GetAndAssert(handler, uri, Energistics.EtpErrorCodes.NotFound);

            // Put Object for Add
            await PutAndAssert(handler, dataObject);

            // Get Added Object
            var args =await GetAndAssert(handler, uri);

            // Check Added Data Object XML
            Assert.IsNotNull(args?.Message.DataObject);
            var xml = args.Message.DataObject.GetString();

            var result = Parse<ChannelSet>(xml);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ExtensionNameValue.FirstOrDefault(e => e.Name.Equals(envName)));

            // Remove Comment from Data Object
            result.ExtensionNameValue.Clear();

            var updateDataObject = CreateDataObject(uri, result);

            // Put Object for Update
            await PutAndAssert(handler, updateDataObject);

            // Get Updated Object
            args = await GetAndAssert(handler, uri);

            // Check Added Data Object XML
            Assert.IsNotNull(args?.Message.DataObject);
            var updateXml = args.Message.DataObject.GetString();

            result = Parse<ChannelSet>(updateXml);
            Assert.IsNotNull(result);

            // Test Data Object overwrite
            Assert.IsNull(result.ExtensionNameValue.FirstOrDefault(e => e.Name.Equals(envName)));
        }

        [TestMethod]
        public async Task ChannelSet200_DeleteObject_Can_Delete_ChannelSet()
        {
            AddParents();
            await RequestSessionAndAssert();

            var handler = _client.Handler<IStoreCustomer>();
            var uri = ChannelSet.GetUri();

            var dataObject = CreateDataObject(uri, ChannelSet);

            // Get Object Expecting it Not to Exist
            await GetAndAssert(handler, uri, Energistics.EtpErrorCodes.NotFound);

            // Put Object
            await PutAndAssert(handler, dataObject);

            // Get Object
            var args = await GetAndAssert(handler, uri);

            // Check Data Object XML
            Assert.IsNotNull(args?.Message.DataObject);
            var xml = args.Message.DataObject.GetString();

            var result = Parse<ChannelSet>(xml);
            Assert.IsNotNull(result);

            // Delete Object
            await DeleteAndAssert(handler, uri);

            // Get Object Expecting it Not to Exist
            await GetAndAssert(handler, uri, Energistics.EtpErrorCodes.NotFound);
        }
    }
}
