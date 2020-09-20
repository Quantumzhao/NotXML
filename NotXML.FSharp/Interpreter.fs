module Interpreter

open TypeDef

let find_id env name = 
    if not (Stdlib.in_stdlib name) then
        let tup = List.find (fun (id, _) -> id = name) env
        match tup with id, value -> value
    else Func_Std(Stdlib.find name)

let rec interp (env : (string * Value) list) (expr : Expr) : Value = 
    match expr with
    | ValLit (vlit) -> Num (decimal (vlit))
    | Str (s) -> Str_Val s
    | Vector (v) -> Vector_Val (List.map (interp env) v)
    | Func (_, body) -> Func_Red body
    | ID (s) -> find_id env s
    | App (id, args) when Stdlib.in_stdlib id -> Stdlib.call id (interp env) args
    | App (id, args) -> 
        let func_body = find_id env id
        let get_func_body_exprs = 
            match func_body with
            | Func_Red (exprs) -> exprs
            | _ -> failwith ""
        let arg_vals = List.map (interp env) args
        let new_env = 
            let get_id index = 
                match get_func_body_exprs.[index] with
                | ID (s) -> s
                | _ -> failwith ""
            let rec expand_env index old_env = 
                if index = -1 then old_env
                else ((get_id index), (arg_vals.[index])) :: expand_env (index - 1) old_env
            expand_env (arg_vals.Length - 1) env
        let func_app = List.find (fun xp -> 
            match xp with 
            | App _ -> true
            | _ -> false) get_func_body_exprs
        let env_w_func_defs =
            get_func_body_exprs
            |> List.filter (fun expr -> 
                match expr with
                | Func _ -> true
                | _ -> false)
            |> List.fold (fun env_acc f ->
                match f with
                | Func (id, _) -> (id, (interp env_acc f)) :: env_acc
                | _ -> failwith "") new_env
        interp env_w_func_defs func_app
