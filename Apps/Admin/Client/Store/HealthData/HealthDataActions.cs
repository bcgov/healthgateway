namespace HealthGateway.Admin.Client.Store.HealthData
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Static class that implements all actions for the feature.
    /// </summary>
    [SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Team decision")]
    public static class HealthDataActions
    {
        /// <summary>
        /// The action representing the request to trigger the process to refresh diagnostic imaging cache.
        /// </summary>
        public record RefreshDiagnosticImagingCacheAction
        {
            /// <summary>
            /// Gets the personal health number (PHN) to check for patient's refresh status.
            /// </summary>
            public required string Phn { get; init; }
        }

        /// <summary>
        /// The action representing the request to trigger the process to refresh laboratory cache.
        /// </summary>
        public record RefreshLaboratoryCacheAction
        {
            /// <summary>
            /// Gets the personal health number (PHN) to check for patient's refresh status.
            /// </summary>
            public required string Phn { get; init; }
        }

        /// <summary>
        /// The action representing a successful request to trigger the process to refresh diagnostic imaging cache.
        /// </summary>
        public record RefreshDiagnosticImagingCacheSuccessAction;

        /// <summary>
        /// The action representing a failed request to trigger the process to refresh diagnostic imaging cache.
        /// </summary>
        public record RefreshDiagnosticImagingCacheFailureAction : BaseFailureAction;

        /// <summary>
        /// The action representing a successful request to trigger the process to refresh laboratory cache.
        /// </summary>
        public record RefreshLaboratoryCacheSuccessAction;

        /// <summary>
        /// The action representing a failed request to trigger the process to refresh laboratory cache.
        /// </summary>
        public record RefreshLaboratoryCacheFailureAction : BaseFailureAction;

        /// <summary>
        /// The action that clears the state.
        /// </summary>
        public record ResetStateAction;
    }
}
