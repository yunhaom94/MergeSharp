# MergeSharp

This library will allow developers to use CRDTs within their .NET application. More specifically, developers can:

-   Choose and create MergeSharp CRDTs
-   Query the states of the CRDTs
-   Update the states of the CRDTs
-   Merge the states of the CRDTs

Each MergeSharp CRDT consists of two classes: The CRDT Message Class and the CRDT class itself. The CRDT Message Class contains the state that is propagated to other CRDTs so that they are able to merge states. The CRDT class itself contains all of the query, update and merging methods.

## Two-Phase (TP) Set Message Class

### Create a TP Set Message:
#### `public TPSetMsg()`, `public TPSetMsg(HashSet<T> addSet, HashSet<T> removeSet)`

### Encodes a TP Set Message from the TP Set Message `addSet` and `removeSet` members to a byte array:
#### `public override byte[] Encode()`

### Decodes a TP Set Message from a byte array into the TP Set Message `addSet` and `removeSet` members:
#### `public override void Decode(byte[] input)`


### Example of using TP Set Message Class:
```
// Get the latest state and encode it
var encodedMsg = set.GetLastSynchronizedUpdate().Encode(); 

// Create a new TP Set Message
TPSetMsg<string> decodedMsg = new();                       

// Decode the TP Set Message
decodedMsg.Decode(encodedMsg);                             
```


## Two-Phase (TP) Set Class

### Create a TP Set:

#### `public TPSet()`

This constructor creates an empty TP Set.

```
TPSet<T> myTPSet = new (); // <T> is a generic type parameter, this must be replaced
```

### Get the Count of a TP Set:

```
myTPSet.Count
```

### Get read-only value of a TP Set:

```
myTPSet.IsReadOnly
```

### Add an `item` to a TP Set:

#### `public virtual void Add (T item)`

This method adds an `item` to the TP Set. If the `item` already exists, nothing is added.

```
TPSet<string> myTPSet = new ();
myTPSet.Add("i love crdts");
```

### Remove an `item` from a TP Set:

#### `public virtual bool Remove (T item)`

This method returns `true` if the `item` was removed, otherwise returns `false`.

```
TPSet<string> myTPSet = new ();
myTPSet.Add("i love crdts");
myTPSet.Remove("hi hi hi hi"); // False
myTPSet.Remove("i love crdts"); // True
```

### Get all items in a TP Set:

#### `public List<T> LookupAll()`

This method returns a `List` of the items in the TP Set.

```
TPSet<int> myTPSet = new ();
myTPSet.Add(3);
myTPSet.Add(5);
myTPSet.Add(7);

List<int> result = myTPSet.LookupAll(); // 3, 5, 7
```

### Compare two TP Sets:

#### `public override bool Equals(object obj)`

This method compares the `items` in the TP Set with the `items` in `obj`. If the items in `obj` are also in the items of the TP Set, the method returns `true`, otherwise `false`.

```
TPSet<int> myTPSet1 = new ();
TPSet<int> myTPSet2 = new ();


myTPSet1.Add(10);
myTPSet2.Add(10);

Console.WriteLine(myTPSet1.Equals(myTPSet2)); // True

myTPSet2.Add(11);

Console.WriteLine(myTPSet1.Equals(myTPSet2)); // False
```

### Check if `item` is in a TP Set:
#### `public bool Contains (T item)`
This method checks if `item` is in the TP Set.
```
TPSet<string> myTPSet = new ();
myTPSet.Add("i love crdts");
myTPSet.Contains("hi hi hi hi"); // False
myTPSet.Contains("i love crdts"); // True
```

### Get the latest state of a TP Set as a `PropagationMessage`:
#### ` public override PropagationMessage GetLastSynchronizedUpdate()`
This method gets the latest state of the TP Set. This method returns a `PropagationMessage`.
```
PropagationMessage newPropMsg = myTPSet.GetLastSynchronizedUpdate();
```

### Apply a state to a TP Set using a `PropagationMessage`:
#### `public override void ApplySynchronizedUpdate(PropagationMessage ReceivedUpdate)`
This method applies `ReceivedUpdate` PropagationMessage as an update to a TP Set.
```
TPSet<string> set = new();
set.Add("a");
set.Add("b");

TPSet<string> set2 = new();
set2.Add("c");
set2.Add("d");
set2.Remove("c");

set.ApplySynchronizedUpdate(set2.GetLastSynchronizedUpdate());
```

### Merge the states of a TP Set using TP Set Message:
#### `public void Merge(TPSetMsg<T> received)`
This method applies `received` TP Set Message as an update to the TP Set.
```
TPSet<string> set = new();
set.Add("a");
set.Add("b");

TPSet<string> set2 = new();
set2.Add("c");
set2.Add("d");
set2.Remove("c");

set.Merge((TPSetMsg<string>)set2.GetLastSynchronizedUpdate());
```

### Get the hash code of a TP Set:
#### `public override int GetHashCode()`
```
myTPSet.GetHashCode();
```