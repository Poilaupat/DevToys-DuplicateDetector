﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DuplicateDetector.Tests.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("DuplicateDetector.Tests.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to un
        ///deux
        ///un
        ///trois
        ///un.
        /// </summary>
        internal static string Deduplicate_LineTest {
            get {
                return ResourceManager.GetString("Deduplicate_LineTest", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Milou
        ///Idefix
        ///Rantanplan
        ///Milou
        ///
        ///Rex
        ///Lassie
        ///Belle
        ///
        ///Rex
        ///Rex
        ///
        ///Rintintin
        ///Chaou
        ///Rantanplan.
        /// </summary>
        internal static string LineTest_ActualDuplicate {
            get {
                return ResourceManager.GetString("LineTest_ActualDuplicate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Milou [1,4]
        ///Rantanplan [3,15]
        ///Rex [6,10,11].
        /// </summary>
        internal static string LineTest_ActualDuplicate_Expected {
            get {
                return ResourceManager.GetString("LineTest_ActualDuplicate_Expected", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to un
        ///deux
        ///trois
        ///quatre
        ///cinq
        ///six
        ///sept
        ///huit
        ///neuf.
        /// </summary>
        internal static string LineTest_NoDuplicate {
            get {
                return ResourceManager.GetString("LineTest_NoDuplicate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 011111000000000000000000000a
        ///022222000000000000000000000b
        ///033333000000000000000000000c
        ///011111000000000000000000000d
        ///044444000000000000000000000e
        ///055555000000000000000000000f
        ///066666000000000000000000000g
        ///077777000000000000000000000h
        ///088888000000000000000000000i
        ///099999000000000000000000000j
        ///0aaaaa000000000000000000000k
        ///0bbbbb00000000000000000000l
        ///0ccccc000000000000000000000m
        ///0ddddd00000000000000000000n
        ///0eeeee000000000000000000000o
        ///011111000000000000000000000p
        ///0aaaaa000000000000000000000q.
        /// </summary>
        internal static string OffsetLengthTest {
            get {
                return ResourceManager.GetString("OffsetLengthTest", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 11111 [1,4,16]
        ///aaaaa [11,17].
        /// </summary>
        internal static string OffsetLengthTest_ActualDuplicate_Expected {
            get {
                return ResourceManager.GetString("OffsetLengthTest_ActualDuplicate_Expected", resourceCulture);
            }
        }
    }
}
