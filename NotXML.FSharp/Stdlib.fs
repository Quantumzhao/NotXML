module Stdlib

open TypeDef
open System

let to_bool value =
    match value with
    | Num (n) -> n <> 0m
    | Str_Val (s) -> s <> null
    | Vector_Val (_ :: _) -> true
    | Vector_Val _ -> false
    | _ -> failwith ""

let to_string value = value.ToString()

let to_dec value =
    match value with
    | Num (n) -> n
    | _ -> failwith ""

let std_if interp_w_env args : Value = 
    match args with
    | cond :: th :: el :: _ -> 
        if cond |> interp_w_env |> to_bool then interp_w_env th
        else interp_w_env el
    | _ -> failwith ""

let rec std_print (interp_w_env : Expr -> Value) args : Value = 
    match args with
    | hd :: tl -> 
        Console.WriteLine (interp_w_env hd)
        std_print interp_w_env tl
    | _ -> Num (1m)

let std_scan interp_w_env args =
    let input = Console.ReadLine()
    match Decimal.TryParse input with
    | true, out -> Num (out)
    | false, _ -> Str_Val (input)

let rec std_add interp_w_env (args : Expr list) : Value = 
    match args with
    | a1 :: a2 :: tl -> 
        let res = 
              (a1 |> interp_w_env |> to_dec)
            + (a2 |> interp_w_env |> to_dec)
            + (to_dec (std_add interp_w_env tl))
        Num (res)
    | hd :: [] -> interp_w_env hd
    | _ -> Num (0m)

let std_sub interp_w_env args =
    match args with
    | a1 :: a2 :: _ -> Num ((a1 |> interp_w_env |> to_dec) - (a2 |> interp_w_env |> to_dec))
    | _ -> failwith ""

let std_lt interp_w_env args =
    match args with
    | left :: right :: _ -> 
        if (left |> interp_w_env |> to_dec) < (right |> interp_w_env |> to_dec) then Num (1m)
        else Num (0m)
    | _ -> failwith ""
    
let find name = 
    match name with
    | "if" -> std_if
    | "add" -> std_add
    | "sub" -> std_sub
    | "print" -> std_print
    | "scan" -> std_scan
    | "lt" -> std_lt
    | _ -> failwith ""

let in_stdlib name = 
    try 
        ignore (find name)
        true
    with _ -> false

let call name interp_w_env args = (find name) interp_w_env args