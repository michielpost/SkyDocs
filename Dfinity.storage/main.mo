import Map "mo:base/HashMap";
import Text "mo:base/Text";
import Principal "mo:base/Principal";

shared(msg) actor class DataList() {

  let owner = msg.caller;

  type Name = Text;
  type Data = Text;

  let data = Map.HashMap<Name, Data>(0, Text.equal, Text.hash);

  public shared(msg) func insert(name : Name, entry : Data): async () {
    let userId = Principal.toText(msg.caller);
     data.put(userId#name, entry);
  };

  public shared(msg) func lookup(name : Name) : async ?Data {
    let userId = Principal.toText(msg.caller);
    data.get(userId#name)
  };

  // Return the principal identifier of the caller of this method.
  public shared (msg) func whoami() : async Text {
    return Principal.toText(msg.caller);
  };


}

