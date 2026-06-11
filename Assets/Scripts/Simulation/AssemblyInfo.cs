using System.Runtime.CompilerServices;

// Test assembly may use internal seams (e.g. L.Seed / L.Clear) without widening the public API.
[assembly: InternalsVisibleTo("Settlers.Tests")]
