namespace rec NBomber

open System
open System.Linq
open System.Collections.Generic
open System.Threading
open System.Threading.Tasks
open System.Runtime.InteropServices

type StepName = string

type Step = {
    StepName: StepName    
    Execute: unit -> Task
} with
  static member Create(name: StepName, execute: Func<Task>) =
    { StepName = name; Execute = execute.Invoke }

  static member CreatePause(delay: TimeSpan) =    
    { StepName = "pause"; Execute = (fun () -> Task.Delay(delay)) }

type TestFlow = {
    FlowName: string
    Steps: Step[]
    ConcurrentCopies: int
}

type Scenario = {
    ScenarioName: string
    InitStep: Step option
    Flows: TestFlow[]
    Interval: TimeSpan
}

type ScenarioBuilder(scenarioName: string) =
    
    let flows = Dictionary<string, TestFlow>()
    let mutable initStep = None    

    let validateFlow (flow) =
        let uniqCount = flow.Steps |> Array.map(fun c -> c.StepName) |> Array.distinct |> Array.length
        
        if flow.Steps.Length <> uniqCount then
            failwith "all steps in test flow should have unique names"

    member x.Init(initFunc: Func<Task>) =
        let flow = { StepName = "init"; Execute = initFunc.Invoke }
        initStep <- Some(flow)
        x    

    member x.AddTestFlow(flow: TestFlow) =
        validateFlow(flow)        
        flows.[flow.FlowName] <- flow
        x

    member x.AddTestFlow(name: string, steps: Step[], concurrentCopies: int) =
        let flow = { FlowName = name; Steps = steps; ConcurrentCopies = concurrentCopies }
        x.AddTestFlow(flow)

    member x.Build(interval: TimeSpan) =
        let testFlows = flows
                        |> Seq.map (|KeyValue|)
                        |> Seq.map (fun (name,job) -> job)
                        |> Seq.toArray

        { ScenarioName = scenarioName
          InitStep = initStep
          Flows = testFlows
          Interval = interval }