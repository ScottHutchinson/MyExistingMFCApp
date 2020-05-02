module NG_DART_Elmish.MsgTypeFilters

open System
open Elmish
open Elmish.WPF

[<AutoOpen>]
module Domain =

    type FieldId = FieldId of Guid

    type MessageType =
        { ID: int
          Name: string
        }

    type FieldData = // Leaf data
        { Id: FieldId
          Name: string
          Type: string
          IsGml: bool
          IsCml: bool
          //IsEntity: bool
          //IsChangeField: bool
        }

    type Field = 
        { Data: FieldData
          Fields: Field list
        }

    type Model =
        { MsgType: MessageType
          ParentStruct: Field
        }

module FieldData =
    let empty () = { Id = Guid.NewGuid () |> FieldId; Name = ""; Type = ""; IsGml = true; IsCml = false; }
    let create name typ = { Id = Guid.NewGuid () |> FieldId; Name = name; Type = typ; IsGml = true; IsCml = false; }
    let setIsGml isChecked fld = { fld with IsGml = isChecked }
    let setIsCml isChecked fld = { fld with IsCml = isChecked }

module Tree =

    let asLeaf a =
        { Data = a
          Fields = [] 
        }

    let dataOfChildren n =
        n.Fields |> List.map (fun nn -> nn.Data)

    let rec mapData f n =
        { Data = n.Data |> f
          Fields = n.Fields |> List.map (mapData f) 
        }

    let asDummyRoot ns =
        { Data = FieldData.empty() // Placeholder data to satisfy type system. User never sees this.
          Fields = ns 
        }

module App =

    let init (msgTypeID: int) (msgTypeName: string) (parentStructName: string) () =
        let child1 = (FieldData.create "XOPERATORX_Weapon_System_Launcher_Blind_Zones_t" "Struct_Base") |> Tree.asLeaf
        let grandchild1 = (FieldData.create "hdr" "Msg_Hdrs_t") |> Tree.asLeaf
        let child1' = { child1 with Fields = grandchild1 :: child1.Fields }
        let dummyRoot = [ child1' ] |> Tree.asDummyRoot
        { MsgType = { ID = msgTypeID; Name = msgTypeName }
          ParentStruct = dummyRoot
        }

    let topLevelFields m =
        m.ParentStruct |> Tree.dataOfChildren

    type Msg =
        | GmlSetChecked of FieldId * bool
        | CmlSetChecked of FieldId * bool

  /// Updates the field using the specified function if the ID matches,
  /// otherwise passes the field through unmodified.
    let updateField f id fld =
        if fld.Id = id then f fld else fld

    let setIsGml isChecked = updateField <| FieldData.setIsGml isChecked
    let setIsCml isChecked = updateField <| FieldData.setIsCml isChecked

    let update msg m =
        match msg with
        | GmlSetChecked (id, isChecked) ->
            { m with ParentStruct = m.ParentStruct |> Tree.mapData (setIsGml isChecked id) }
        | CmlSetChecked (id, isChecked) ->
            { m with ParentStruct = m.ParentStruct |> Tree.mapData (setIsCml isChecked id) }

    let rec fieldBindings () : Binding<Model * FieldData, Msg> list = [
        "Name" |> Binding.oneWay(fun (_, fd) -> fd.Name)
        //"IsGml" |> Binding.twoWay(
        //    (fun (_, fd) -> fd.IsGml),
        //    (fun v (_, fd) -> setIsGml v (fd.Id fd))
        //)
    ]

    let rootBindings () : Binding<Model, Msg> list = [
        "MsgTypeID" |> Binding.oneWay(fun m -> m.MsgType.ID)
        "Fields" |> Binding.subModelSeq(
          (fun m -> m |> topLevelFields),
          (fun (fd:FieldData) -> fd.Id), 
          fieldBindings
        )
    ]

module PublicAPI =
    open NG_DART_WPF

    let LoadWindow (msgTypeID: int) (msgTypeName: string) (parentStructName: string) =
        let init = App.init msgTypeID msgTypeName parentStructName
        Program.mkSimpleWpf init App.update App.rootBindings
        |> Program.withConsoleTrace
        |> Program.runWindowWithConfig
            { ElmConfig.Default with LogConsole = true; Measure = true }
            (MsgTypeFiltersWindow())
