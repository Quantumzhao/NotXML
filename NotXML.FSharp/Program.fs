// Learn more about F# at http://fsharp.org

open TypeDef
open Parser
open Interpreter

[<EntryPoint>]
let main argv =
    let url = System.IO.Directory.GetCurrentDirectory() + "/" + argv.[0]
    let expr_main = url |> rootNode |> parse |> interp []
    let run = App ("main", [])
    ignore (interp [("main", expr_main)] run);
    
    0 // return an integer exit code
