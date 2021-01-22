﻿// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;

namespace Umbraco.Web.Common.Profiler
{
    /// <summary>
    /// Initialized the web profiling. Ensures the boot process profiling is stopped.
    /// </summary>
    public class InitializeWebProfiling : INotificationHandler<UmbracoApplicationStarting>, INotificationHandler<UmbracoRequestBegin>,  INotificationHandler<UmbracoRequestEnd>
    {
        private readonly bool _profile;
        private readonly WebProfiler _profiler;

        /// <summary>
        /// Initializes a new instance of the <see cref="InitializeWebProfiling"/> class.
        /// </summary>
        public InitializeWebProfiling(IProfiler profiler, ILogger<InitializeWebProfiling> logger)
        {
            _profile = true;

            // although registered in UmbracoBuilderExtensions.AddUmbraco, ensure that we have not
            // been replaced by another component, and we are still "the" profiler
            _profiler = profiler as WebProfiler;
            if (_profiler != null)
            {
                return;
            }

            // if VoidProfiler was registered, let it be known
            if (profiler is NoopProfiler)
            {
                logger.LogInformation(
                    "Profiler is VoidProfiler, not profiling (must run debug mode to profile).");
            }

            _profile = false;
        }

        /// <inheritdoc/>
        public Task HandleAsync(UmbracoApplicationStarting notification, CancellationToken cancellationToken)
        {
            if (_profile)
            {
                // Stop the profiling of the booting process
                _profiler.StopBoot();
            }

            return Task.CompletedTask;
        }

        public Task HandleAsync(UmbracoRequestBegin notification, CancellationToken cancellationToken)
        {
            if (_profile)
            {
                _profiler.UmbracoApplicationBeginRequest(notification.HttpContext);
            }

            return Task.CompletedTask;
        }

        public Task HandleAsync(UmbracoRequestEnd notification, CancellationToken cancellationToken)
        {
            if (_profile)
            {
                _profiler.UmbracoApplicationEndRequest(notification.HttpContext);
            }

            return Task.CompletedTask;
        }
    }
}
