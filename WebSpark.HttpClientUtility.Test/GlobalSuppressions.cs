// Global suppression file for test project
// Tests don't need to be trim-compatible or AOT-compatible

using System.Diagnostics.CodeAnalysis;

// Suppress trimming warnings in test code - tests are not trimmed or AOT compiled
[assembly: UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Test projects do not need to be trim-compatible")]
[assembly: UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Test projects do not need to be AOT-compatible")]
[assembly: UnconditionalSuppressMessage("Trimming", "IL2090", Justification = "Test projects do not need to be trim-compatible")]
