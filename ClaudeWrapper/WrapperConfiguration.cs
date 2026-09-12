using TruePath;

namespace ClaudeWrapper;

public record WrapperConfiguration(
    Dictionary<LocalPathPattern, AbsolutePath> ConfigDirectoriesPerPath
);
