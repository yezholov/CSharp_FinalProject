# CSharp_FinalProject

**Objective**: Create a distributed system consisting of three separate C# console applications

**Author**: Kyrylo Yezholov <kyrylo.yezholov@sa.stud.vu.lt> @yezholov @kirillezh

## Commit history:

**Commit 0**: Create projects Agent and Master. Add .gitignore. Update README.md

**Commit 1**: Add File Structure to Agent and Master

**Commit 2**: A basic asynchronous Agent that indexes files has been created. Sample test text has been automatically generated for testing the application.
To test this version, type: `dotnet run ../TestData`

**Commit 3:** Created a basic PipeClient in Agent for communication between programs. Update `Program.cs.`
To test this version, type: `dotnet run ../TestData <AgentID>.`

**Commit 4**: Created a basic Master to retrieve data from the Agent. Create `PipeServer.cs`, `Program.cs`.
To test this version first start the Agent, type: `dotnet run ../TestData 1`; then start the Master, type: `dotnet run. `

**Commit 5**: Created `AgentData`, `AggregatedIndex`, `DataAggregator` and `ResultPrinter`, for grouping and sorting data within the data of one file. Update Program.cs.
To test this version first start the Agent, type: `dotnet run ../TestData 1`; then start the Master, type: `dotnet run. `
