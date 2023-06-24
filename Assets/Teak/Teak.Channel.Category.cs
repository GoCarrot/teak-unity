#region References
/// @cond hide_from_doxygen
using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;

using MiniJSON.Teak;
using TeakExtensions;
/// @endcond
#endregion

public partial class Teak {
    public partial class Channel {
        /// <summary>The id, name and description of a Teak marketing channel category</summary>
        public class Category {
            public string Id {
                get; private set;
            }

            public string Name {
                get; private set;
            }

            public string Description {
                get; private set;
            }

            public Category(string id, string name, string description) {
                this.Id = id;
                this.Name = name;
                this.Description = description;
            }

            /// <summary>
            /// Returns a string that represents the current object.
            /// </summary>
            /// <returns>A string that represents the current object.</returns>
            public override string ToString() {
                string formatString = "{{ Id = '{0}', Name = '{1}', Description = '{2}' }}";
                return string.Format(formatString, this.Id, this.Name, this.Description);
            }

        }
    }
}
