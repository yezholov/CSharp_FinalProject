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

**Commit 6**: Added support for multiple Agents, improved error handling. Updated `Program.cs`, improved output of multiple files in `ResultPrinter.cs`. Added aditional file with text: `test_data2.txt`.
To test this version first start the first Agent, type: `dotnet run ../TestData 1` and the second Agent, type: `dotnet run ../TestData 2`; then start the Master, type: `dotnet run. `

**Commit 7**: Code refactoring. Updated logging. Simplified Agent and Master work. Agent: receives a directory from argument and send indices from all files in the directory via pipe. Master: receives information about all files from all directories and outputs data.
*Updated files*: WordIndex.cs (added output of indices to string), PipeClient.cs (removed support for receiving data, changed end of data handling, improved logging), PipeServer.cs (code refactoring, removed support for sending data, improved processing of received data, improved logging), also updated both Program.cs files to support the required functionality, improved error handling, improved logging.
To test this version first start the first Agent, type: `dotnet run ../TestData 1` and the second Agent, type: `dotnet run ../TestData2 2`; then start the Master, type: `dotnet run. `*In this update, you can run files in any order.*

**Сommit 8**: Updated `Agent`. Separated file processing and data sending into different threads for multithreading. Added `BlockingCollection` for better data exchange between tasks. Updated logging. Added new data set.
To test this version first start the first Agent, type: `dotnet run ../TestDataA 1` and the second Agent, type: `dotnet run ../TestDataB 2`; then start the Master, type: `dotnet run. `*In this update, you can run files in any order.*

**Commit 9**: Added multi-core support. Each program runs on a separate core (if there are enough of them), for Master 1st core, for agents NumberOfAgent+1.
**Attention**: ProcessorAffinity only supported on *Linux* and *Windows*. *MacOS* will not run this solution, so in *MacOS* multi-ciore suppot is disabled automatically.
To test this version first start the first Agent, type: `dotnet run ../TestDataA 1` and the second Agent, type: `dotnet run ../TestDataB 2`; then start the Master, type: `dotnet run. `*In this update, you can run files in any order.*

**Commit 10**: Added autostart of *Agents* via *Master* with `Process` (*tested in macOS, errors are possible on other platforms*). Updated logging.
To test this version firsly (only one time) build Agent, type: `dotnet build` (or you can type `dotnet run`, to build and run), finally run Master, type: `dotnet run.`
