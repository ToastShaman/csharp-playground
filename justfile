test: fmt
    dotnet test

fmt:
    # dotnet tool install csharpier -g
    dotnet csharpier .
