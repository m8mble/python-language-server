﻿// Copyright(c) Microsoft Corporation
// All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the License); you may not use
// this file except in compliance with the License. You may obtain a copy of the
// License at http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED ON AN  *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS
// OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY
// IMPLIED WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE,
// MERCHANTABILITY OR NON-INFRINGEMENT.
//
// See the Apache Version 2.0 License for specific language governing
// permissions and limitations under the License.

using System.Diagnostics;
using Microsoft.Python.Analysis.Analyzer;
using Microsoft.Python.Core;
using Microsoft.Python.Core.Services;

namespace Microsoft.Python.LanguageServer.Implementation {
    public sealed partial class Server {
        private void OnAnalysisComplete(object sender, AnalysisCompleteEventArgs e) {
            if (e.MillisecondsElapsed < 500) {
                return;
            }
            var telemetry = _services.GetService<ITelemetryService>();
            if (telemetry == null) {
                return;
            }

            double privateMB;
            double peakPagedMB;
            double workingMB;

            using (var proc = Process.GetCurrentProcess()) {
                privateMB = proc.PrivateMemorySize64 / 1e+6;
                peakPagedMB = proc.PeakPagedMemorySize64 / 1e+6;
                workingMB = proc.WorkingSet64 / 1e+6;
            }

            var te = new TelemetryEvent {
                EventName = "python_language_server/analysis_complete", // TODO: Move this common prefix into Core.
            };

            te.Measurements["privateMB"] = privateMB;
            te.Measurements["peakPagedMB"] = peakPagedMB;
            te.Measurements["workingMB"] = workingMB;
            te.Measurements["elapsedMs"] = e.MillisecondsElapsed;
            te.Measurements["moduleCount"] = e.ModuleCount;

            telemetry.SendTelemetryAsync(te).DoNotWait();
        }
    }
}
