namespace Tests.TestHelper

open Serilog
open Serilog.Sinks.InMemory

open NBomber
open NBomber.Contracts
open NBomber.Contracts.Stats
open NBomber.Infra
open NBomber.Infra.Dependency
open NBomber.Infra.Logger
open NBomber.DomainServices

module internal Dependency =

    let createFor (nodeType: NodeType) =

        let testInfo = {
            SessionId = Dependency.createSessionId()
            TestSuite = Constants.DefaultTestSuite
            TestName = Constants.DefaultTestName
            ClusterId = ""
        }

        let emptyContext = NBomberContext.empty

        let logSettings = {
            Folder = "./reports"
            TestInfo = testInfo
            NodeType = nodeType
            AgentGroup = ""
        }

        let dep = Dependency.create ApplicationType.Process logSettings emptyContext
        {| TestInfo = testInfo; Dep = dep |}

    let createWithInMemoryLogger (nodeType: NodeType) =

        let testInfo = {
            SessionId = Dependency.createSessionId()
            TestSuite = Constants.DefaultTestSuite
            TestName = Constants.DefaultTestName
            ClusterId = ""
        }

        let inMemorySink = new InMemorySink()
        let loggerConfig = fun () -> LoggerConfiguration().WriteTo.Sink(inMemorySink)
        let context = { NBomberContext.empty with CreateLoggerConfig = Some loggerConfig }

        let logSettings = {
            Folder = "./reports"
            TestInfo = testInfo
            NodeType = nodeType
            AgentGroup = ""
        }

        let dep = Dependency.create ApplicationType.Process logSettings context

        let dependency = {
            new IGlobalDependency with
                member _.ApplicationType = dep.ApplicationType
                member _.NodeType = dep.NodeType
                member _.NBomberConfig = dep.NBomberConfig
                member _.InfraConfig = dep.InfraConfig
                member _.CreateLoggerConfig = dep.CreateLoggerConfig
                member _.Logger = dep.Logger
                member _.ReportingSinks = dep.ReportingSinks
                member _.WorkerPlugins = dep.WorkerPlugins }

        {| TestInfo = testInfo
           Dep = dependency
           MemorySink = inMemorySink |}

module List =

    /// Safe variant of `List.min`
    let minOrDefault defaultValue list =
        if List.isEmpty list then defaultValue
        else List.min list

    /// Safe variant of `List.max`
    let maxOrDefault defaultValue list =
        if List.isEmpty list then defaultValue
        else List.max list

    /// Safe variant of `List.average`
    let averageOrDefault (defaultValue: float) list =
        if List.isEmpty list then defaultValue
        else list |> List.average
