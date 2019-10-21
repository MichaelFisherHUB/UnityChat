using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property |
    AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
public class ReadOnlyAttribute : PropertyAttribute
{

}
