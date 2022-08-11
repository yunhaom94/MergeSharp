using System;
using System.Collections.Generic;

namespace KVDB;

public class KeySpaceManager
{
    public Dictionary<string, Guid> keySpaces;

    public KeySpaceManager()
    {
        keySpaces = new Dictionary<string, Guid>();
    }


}
