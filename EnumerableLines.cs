using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace PlayByPlay
{
	class EnumerableLines : IEnumerable<string>
	{
		private class LineEnumerator : IEnumerator<string>
		{
			private StreamReader _reader;

			public string Current
			{
				get; private set;
			}

			object IEnumerator.Current => Current;

			public LineEnumerator(StreamReader reader)
			{
				_reader = reader ?? throw new ArgumentNullException(nameof(reader));
			}

			public void Dispose()
			{
				_reader?.Dispose();
			}

			public bool MoveNext()
			{
				if (_reader.EndOfStream)
				{
					return false;
				}
				Current = _reader.ReadLine();

				return Current != null;
			}

			public void Reset()
			{
				throw new NotImplementedException();
			}
		}

		private Stream _stream;

		public EnumerableLines(Stream stream)
		{
			_stream = stream ?? throw new ArgumentNullException(nameof(stream));
		}

		public IEnumerator<string> GetEnumerator() => new LineEnumerator(new StreamReader(_stream));

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}