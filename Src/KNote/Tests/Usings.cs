global using Microsoft.VisualStudio.TestTools.UnitTesting;

// The in-process (WebApplicationFactory-based) test classes configure their Server host via
// process-wide environment variables (see KNoteWebApplicationFactory), so test classes must not
// run concurrently - parallel class execution would race on those environment variables.
[assembly: DoNotParallelize]