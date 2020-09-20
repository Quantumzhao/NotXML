module Parser

open TypeDef
open System.Xml
open System

let rootNode (path : string) : XmlElement = 
    let doc = new XmlDocument()
    doc.Load(path);
    doc.DocumentElement

let toList (c : XmlNodeList) = [for e in c do yield e]

let is_val (str : string) = 
    str.[0] = '\"' ||
    match Decimal.TryParse(str) with
    | true, out -> true
    | false, _ -> false

let rec parse (node : XmlNode) : Expr = 
    let attr = node.Attributes
    match node.Name with
    | "fun" -> Func (node.Attributes.["id"].Value, (List.map parse (toList node.ChildNodes)))
    | "val" when node.InnerText |> is_val |> not -> ID (node.InnerText)
    | "par" -> ID (node.Attributes.["id"].Value)
    | "app" -> App (node.Attributes.["id"].Value, List.map parse (toList node.ChildNodes))
    | "val" -> ValLit (node.InnerText)
    | _ -> failwith "No such type"