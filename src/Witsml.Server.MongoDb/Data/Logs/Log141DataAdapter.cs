﻿//----------------------------------------------------------------------- 
// PDS.Witsml.Server, 2016.1
//
// Copyright 2016 Petrotechnical Data Systems
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Energistics.DataAccess;
using Energistics.DataAccess.WITSML141;
using Energistics.DataAccess.WITSML141.ComponentSchemas;
using Energistics.DataAccess.WITSML141.ReferenceData;
using Energistics.Datatypes;
using Energistics.Datatypes.ChannelData;
using MongoDB.Driver;
using PDS.Framework;
using PDS.Witsml.Data.Channels;
using PDS.Witsml.Data.Logs;
using PDS.Witsml.Server.Data.Channels;

namespace PDS.Witsml.Server.Data.Logs
{
    /// <summary>
    /// Data adapter that encapsulates CRUD functionality for a 141 <see cref="Log" />
    /// </summary>
    /// <seealso cref="PDS.Witsml.Server.Data.Logs.LogDataAdapter{Log, LogCurveInfo}" />
    [Export(typeof(IWitsmlDataAdapter<Log>))]
    [Export141(ObjectTypes.Log, typeof(IChannelDataProvider))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class Log141DataAdapter : LogDataAdapter<Log, LogCurveInfo>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Log141DataAdapter"/> class.
        /// </summary>
        /// <param name="databaseProvider">The database provider.</param>
        [ImportingConstructor]
        public Log141DataAdapter(IDatabaseProvider databaseProvider) : base(databaseProvider, ObjectNames.Log141)
        {
        }

        /// <summary>
        /// Queries the specified <see cref="Log"/> in the store.
        /// </summary>
        /// <param name="parser">The parser that specifies the query parameters.</param>
        /// <returns>Queried objects.</returns>
        public override WitsmlResult<IEnergisticsCollection> Query(WitsmlQueryParser parser)
        {
            var logcurveInfos = GetLogCurveInfoMnemonics(parser).ToList();
            var mnemonicList = GetLogDataMnemonics(parser).ToList();

            if (OptionsIn.ReturnElements.Requested.Equals(parser.ReturnElements()))
            {
                if (logcurveInfos.Any() && mnemonicList.Any() && !(logcurveInfos.All(x => mnemonicList.Contains(x)) && mnemonicList.All(y => logcurveInfos.Contains(y))))
                {
                    throw new WitsmlException(ErrorCodes.ColumnIdentifiersNotSame);
                }

                if (parser.Contains("logCurveInfo"))
                {
                    var properties = parser.Properties("logCurveInfo").ToArray();
                    if (properties.Any(x => x.IsEmpty))
                    {
                        throw new WitsmlException(ErrorCodes.MissingMnemonicElement);
                    }
                }

                if (parser.Contains("logData") && !mnemonicList.Any())
                {
                    throw new WitsmlException(ErrorCodes.MissingMnemonicList);
                }
            }

            return base.Query(parser);
        }

        /// <summary>
        /// Adds a <see cref="Log" /> entity to the data store.
        /// </summary>
        /// <param name="parser">The input template parser.</param>
        /// <param name="dataObject">The <see cref="Log" /> to be added.</param>
        /// <returns>
        /// A WITSML result that includes a positive value indicates a success or a negative value indicates an error.
        /// </returns>
        public override WitsmlResult Add(WitsmlQueryParser parser, Log dataObject)
        {
            // Extract Data                    
            var reader = ExtractDataReaders(dataObject).FirstOrDefault();

            // Insert Log and Log Data
            InsertEntity(dataObject);
            InsertLogData(dataObject, reader);

            return new WitsmlResult(ErrorCodes.Success, dataObject.Uid);
        }

        /// <summary>
        /// Updates the specified <see cref="Log" /> instance in the store.
        /// </summary>
        /// <param name="parser">The update parser.</param>
        /// <param name="dataObject">The data object to be updated.</param>
        /// <returns>
        /// A WITSML result that includes a positive value indicates a success or a negative value indicates an error.
        /// </returns>
        public override WitsmlResult Update(WitsmlQueryParser parser, Log dataObject)
        {
            var uri = dataObject.GetUri();
            var ignored = GetIgnoredElementNames().Concat(new[] { "direction" }).ToArray();

            UpdateEntity(parser, uri, ignored);

            // Update Log Data and Index Range
            var readers = ExtractDataReaders(dataObject, GetEntity(uri));
            UpdateLogDataAndIndexRange(uri, readers);

            return new WitsmlResult(ErrorCodes.Success);
        }

        protected override IEnergisticsCollection CreateCollection(List<Log> entities)
        {
            return new LogList() { Log = entities };
        }

        protected override object CreateGenericMeasure(double value, string uom)
        {
            return new GenericMeasure() { Value = value, Uom = uom };
        }

        protected override bool IsIncreasing(Log log)
        {
            return log.Direction.GetValueOrDefault() == LogIndexDirection.increasing;
        }

        protected override bool IsTimeLog(Log log, bool includeElapsedTime = false)
        {
            return log.IndexType.GetValueOrDefault() == LogIndexType.datetime ||
                   (log.IndexType.GetValueOrDefault() == LogIndexType.elapsedtime && includeElapsedTime);
        }

        protected override List<LogCurveInfo> GetLogCurves(Log log)
        {
            return log.LogCurveInfo;
        }

        protected override string GetMnemonic(LogCurveInfo curve)
        {
            return curve?.Mnemonic?.Value;
        }

        protected override string GetIndexCurveMnemonic(Log log)
        {
            return log.IndexCurve;
        }

        protected override IDictionary<int, string> GetUnitsByColumnIndex(Log log)
        {
            return log.LogCurveInfo
                .Select(x => x.Unit)
                .ToArray()
                .Select((unit, index) => new { Unit = unit, Index = index })
                .ToDictionary(x => x.Index, x => x.Unit);
        }

        protected override Range<double?> GetIndexRange(LogCurveInfo curve, bool increasing = true, bool isTimeIndex = false)
        {
            return curve.GetIndexRange(increasing, isTimeIndex);
        }

        protected override void SetLogDataValues(Log log, List<string> logDataValues, IEnumerable<string> mnemonics, IEnumerable<string> units)
        {
            if (log.LogData == null)
                log.LogData = new List<LogData>();

            log.LogData.Add(new LogData()
            {
                MnemonicList = string.Join(",", mnemonics),
                UnitList = string.Join(",", units),
                Data = logDataValues
            });
        }

        protected override void SetLogIndexRange(Log log, Dictionary<string, Range<double?>> ranges)
        {
            if (log.LogCurveInfo == null)
                return;

            var isTimeLog = IsTimeLog(log);
            var increasing = IsIncreasing(log);

            foreach (var logCurve in log.LogCurveInfo)
            {
                var mnemonic = logCurve.Mnemonic.Value;
                Range<double?> range;

                if (!ranges.TryGetValue(mnemonic, out range))
                    continue;

                // Sort range in min/max order
                range = range.Sort();

                if (isTimeLog)
                {
                    if (range.Start.HasValue && !double.IsNaN(range.Start.Value))
                        logCurve.MinDateTimeIndex = DateTimeOffset.FromUnixTimeSeconds((long)range.Start.Value);
                    if (range.End.HasValue && !double.IsNaN(range.End.Value))
                        logCurve.MaxDateTimeIndex = DateTimeOffset.FromUnixTimeSeconds((long)range.End.Value);

                    if (mnemonic.EqualsIgnoreCase(log.IndexCurve))
                    {
                        log.StartDateTimeIndex = increasing ? logCurve.MinDateTimeIndex : logCurve.MaxDateTimeIndex;
                        log.EndDateTimeIndex = increasing ? logCurve.MaxDateTimeIndex : logCurve.MinDateTimeIndex;
                    }
                }
                else
                {
                    if (range.Start.HasValue)
                        logCurve.MinIndex.Value = range.Start.Value;
                    if (range.End.HasValue)
                        logCurve.MaxIndex.Value = range.End.Value;

                    if (mnemonic.EqualsIgnoreCase(log.IndexCurve))
                    {
                        log.StartIndex.Value = increasing ? logCurve.MinIndex.Value : logCurve.MaxIndex.Value;
                        log.EndIndex.Value = increasing ? logCurve.MaxIndex.Value : logCurve.MinIndex.Value;
                    }
                }
            }
        }

        protected override UpdateDefinition<Log> UpdateCommonData(MongoDbUpdate<Log> mongoUpdate, UpdateDefinition<Log> logHeaderUpdate, Log entity, TimeSpan? offset)
        {
            if (entity?.CommonData == null)
                return logHeaderUpdate;

            if (entity.CommonData.DateTimeCreation.HasValue)
            {
                var creationTime = entity.CommonData.DateTimeCreation.ToOffsetTime(offset);
                logHeaderUpdate = MongoDbUtility.BuildUpdate(logHeaderUpdate, "CommonData.DateTimeCreation", creationTime?.ToString("o"));
                Logger.DebugFormat("Updating Common Data create time to '{0}'", creationTime);
            }

            if (entity.CommonData.DateTimeLastChange.HasValue)
            {
                var updateTime = entity.CommonData.DateTimeLastChange.ToOffsetTime(offset);
                logHeaderUpdate = MongoDbUtility.BuildUpdate(logHeaderUpdate, "CommonData.DateTimeLastChange", updateTime?.ToString("o"));
                Logger.DebugFormat("Updating Common Data update time to '{0}'", updateTime);
            }

            return logHeaderUpdate;
        }

        protected override IndexMetadataRecord ToIndexMetadataRecord(Log entity, LogCurveInfo indexCurve, int scale = 3)
        {
            return new IndexMetadataRecord()
            {
                Uri = indexCurve.GetUri(entity),
                Mnemonic = indexCurve.Mnemonic.Value,
                Description = indexCurve.CurveDescription,
                Uom = indexCurve.Unit,
                Scale = scale,
                IndexType = IsTimeLog(entity, true)
                    ? ChannelIndexTypes.Time
                    : ChannelIndexTypes.Depth,
                Direction = IsIncreasing(entity)
                    ? IndexDirections.Increasing
                    : IndexDirections.Decreasing,
                CustomData = new Dictionary<string, DataValue>(0),
            };
        }

        protected override ChannelMetadataRecord ToChannelMetadataRecord(Log entity, LogCurveInfo curve, IndexMetadataRecord indexMetadata)
        {
            var uri = curve.GetUri(entity);
            var isTimeLog = IsTimeLog(entity, true);
            var curveIndexes = GetCurrentIndexRange(entity);

            return new ChannelMetadataRecord()
            {
                ChannelUri = uri,
                ContentType = uri.ContentType,
                DataType = curve.TypeLogData.GetValueOrDefault(LogDataType.@double).ToString().Replace("@", string.Empty),
                Description = curve.CurveDescription ?? curve.Mnemonic.Value,
                Mnemonic = curve.Mnemonic.Value,
                Uom = curve.Unit,
                MeasureClass = curve.ClassWitsml ?? ObjectTypes.Unknown,
                Source = curve.DataSource ?? ObjectTypes.Unknown,
                Uuid = curve.Mnemonic.Value,
                Status = ChannelStatuses.Active,
                ChannelAxes = new List<ChannelAxis>(),
                StartIndex = curveIndexes[curve.Mnemonic.Value].Start.IndexToScale(indexMetadata.Scale, isTimeLog),
                EndIndex = curveIndexes[curve.Mnemonic.Value].End.IndexToScale(indexMetadata.Scale, isTimeLog),
                Indexes = new List<IndexMetadataRecord>()
                {
                    indexMetadata
                }
            };
        }

        private IEnumerable<ChannelDataReader> ExtractDataReaders(Log entity, Log existing = null)
        {
            if (existing == null)
            {
                var readers = entity.GetReaders().ToList();
                entity.LogData = null;
                return readers;
            }

            existing.LogData = entity.LogData;
            return existing.GetReaders().ToList();
        }
    }
}
