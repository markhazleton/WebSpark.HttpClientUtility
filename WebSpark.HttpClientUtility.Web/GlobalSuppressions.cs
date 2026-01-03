// Global suppression file for web demo application
// Demo app doesn't need to be trim-compatible or AOT-compatible

using System.Diagnostics.CodeAnalysis;

// Suppress trimming warnings in web demo - not a production application
[assembly: UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Demo application does not need to be trim-compatible")]
[assembly: UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Demo application does not need to be AOT-compatible")]
