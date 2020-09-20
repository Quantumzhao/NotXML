// Learn more about F# at http://fsharp.org

open System
open TypeDef
open Parser
open Interpreter

[<EntryPoint>]
let main argv =
    let url = @"C:\Users\yisha.000\source\repos\NotXML\NotXML\test.notxml"
    let expr_main = url |> rootNode |> parse |> interp []
    let run = App ("main", [])
    Console.WriteLine(interp [("main", expr_main)] run);
    
    0 // return an integer exit code
