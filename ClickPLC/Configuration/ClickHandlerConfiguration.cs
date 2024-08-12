/*
 * Copyright (c) 2024 vasilyevl (Grumpy). Permission is hereby granted, 
free of charge, to any person obtaining a copy of this software
and associated documentation files (the "Software"),to deal in the Software 
without restriction, including without limitation the rights to use, copy, 
modify, merge, publish, distribute, sublicense, and/or sell copies of the 
Software, and to permit persons to whom the Software is furnished to do so, 
subject to the following conditions:

The above copyright notice and this permission notice shall be included 
in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,FITNESS FOR A 
PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION 
OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE 
OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

*/

using Newtonsoft.Json;


namespace Grumpy.ClickPLC.Net.Driver
{
    public interface IClickDriverConfiguration
    {
        IInterfaceConfiguration? Interface { get; set; }
    }


    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class ClickDriverConfiguration : IClickDriverConfiguration, ICloneable
    {
        private InterfaceConfiguration? _interface;

        public ClickDriverConfiguration() : base() {

            Interface = new InterfaceConfiguration();   
        }

        public ClickDriverConfiguration(ClickDriverConfiguration source) : this() {

            this.CopyFrom(source);
        }

        public bool CopyFrom(object src) {

            var s = src as IClickDriverConfiguration;

            if (s == null) { return false; }

            if (s.Interface != null) {

                var tmp = new InterfaceConfiguration();
                tmp.CopyFrom(s.Interface);
                _interface = tmp;
            }
            return true;
        }

        public void Reset() {
            _interface = new InterfaceConfiguration();
        }
        public object Clone() {
            var clone = new ClickDriverConfiguration();
            clone.CopyFrom(this);
            return clone;
        }

        [JsonProperty]
        public IInterfaceConfiguration? Interface {
            get => _interface;
            set => _interface = value as InterfaceConfiguration;
        }
    }
}
