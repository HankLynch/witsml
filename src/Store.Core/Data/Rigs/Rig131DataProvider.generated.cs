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
using System.ComponentModel.Composition;
using System.Xml.Linq;
using Energistics.DataAccess.WITSML131;
using Energistics.DataAccess.WITSML131.ComponentSchemas;
using Energistics.Datatypes;
using PDS.WITSMLstudio.Framework;

namespace PDS.WITSMLstudio.Store.Data.Rigs
{
    /// <summary>
    /// Data provider that implements support for WITSML API functions for <see cref="Rig"/>.
    /// </summary>

    /// <seealso cref="PDS.WITSMLstudio.Store.Data.WitsmlDataProvider{RigList, Rig}" />
    [Export(typeof(IEtpDataProvider))]
    [Export(typeof(IEtpDataProvider<Rig>))]
    [Export131(ObjectTypes.Rig, typeof(IEtpDataProvider))]
    [Export131(ObjectTypes.Rig, typeof(IWitsmlDataProvider))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class Rig131DataProvider : WitsmlDataProvider<RigList, Rig>

    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Rig131DataProvider"/> class.
        /// </summary>
        /// <param name="container">The composition container.</param>
        /// <param name="dataAdapter">The data adapter.</param>
        [ImportingConstructor]
        public Rig131DataProvider(IContainer container, IWitsmlDataAdapter<Rig> dataAdapter) : base(container, dataAdapter)
        {
        }

        /// <summary>
        /// Sets the default values for the specified data object.
        /// </summary>
        /// <param name="dataObject">The data object.</param>
        protected override void SetDefaultValues(Rig dataObject)
        {
            dataObject.Uid = dataObject.NewUid();
            dataObject.CommonData = dataObject.CommonData.Create();

            SetAdditionalDefaultValues(dataObject);
        }

        /// <summary>
        /// Sets the default values for the specified data object.
        /// </summary>
        /// <param name="dataObject">The data object.</param>
        /// <param name="uri">The data object URI.</param>
        protected override void SetDefaultValues(Rig dataObject, EtpUri uri)
        {
            dataObject.Uid = uri.ObjectId;
            dataObject.Name = dataObject.Uid;

            // Wellbore
            var parentUri = uri.Parent;
            dataObject.UidWellbore = parentUri.ObjectId;
            dataObject.NameWellbore = dataObject.UidWellbore;

            // Well
            parentUri = parentUri.Parent;
            dataObject.UidWell = parentUri.ObjectId;
            dataObject.NameWell = dataObject.UidWell;

            SetAdditionalDefaultValues(dataObject, uri);
        }

        /// <summary>
        /// Sets the default values for the specified data object during update.
        /// </summary>
        /// <param name="dataObject">The data object.</param>
        /// <param name="parser">The input template.</param>
        protected override void UpdateDefaultValues(Rig dataObject, WitsmlQueryParser parser)
        {
            UpdateAdditionalDefaultValues(dataObject, parser);
        }

        /// <summary>
        /// Creates a new <see cref="RigList" /> instance containing the specified data objects.
        /// </summary>
        /// <param name="dataObjects">The data objects.</param>
        /// <returns>A new <see cref="RigList" /> instance.</returns>
        protected override RigList CreateCollection(List<Rig> dataObjects)
        {
            return new RigList { Rig = dataObjects };
        }

        /// <summary>
        /// Sets additional default values for the specified data object.
        /// </summary>
        /// <param name="dataObject">The data object.</param>
        partial void SetAdditionalDefaultValues(Rig dataObject);

        /// <summary>
        /// Sets additional default values for the specified data object and URI.
        /// </summary>
        /// <param name="dataObject">The data object.</param>
        /// <param name="uri">The data object URI.</param>
        partial void SetAdditionalDefaultValues(Rig dataObject, EtpUri uri);

        /// <summary>
        /// Sets additional default values for the specified data object during update.
        /// </summary>
        /// <param name="dataObject">The data object.</param>
        /// <param name="parser">The input template.</param>
        partial void UpdateAdditionalDefaultValues(Rig dataObject, WitsmlQueryParser parser);
    }
}
