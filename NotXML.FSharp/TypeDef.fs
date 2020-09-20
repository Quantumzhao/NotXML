module TypeDef

type Expr = 
    | Func of string * Expr list
    | App of string * Expr list
    | ValLit of string
    | Str of string
    | Vector of Expr list
    | ID of string

type Value = 
    | Str_Val of string
    | Num of decimal
    | Vector_Val of Value list
    | Func_Red of Expr list
    | Func_Std of ((Expr -> Value) -> Expr list -> Value)
