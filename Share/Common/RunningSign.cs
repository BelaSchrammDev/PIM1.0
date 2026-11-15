namespace IngameScript
{
    partial class Program
    {
        /// <summary>
        /// Running sign of the PB info: a marker bouncing between two bars, one step per call,
        /// "|>       | ", "| >      | ", ... "|       >| ", "|      < | ", ... "|<       | ".
        /// Pure (no SE types), tested in RunningSignTests.
        /// </summary>
        public class RunningSign
        {
            const int Width = 8;
            int _pos, _dir = 1;

            public string Next()
            {
                _pos += _dir;
                var marker = _dir > 0 ? ">" : "<";
                if (_pos == 0 || _pos == Width - 1) _dir = -_dir;
                return "|" + new string(' ', _pos) + marker + new string(' ', Width - 1 - _pos) + "| ";
            }
        }
    }
}
