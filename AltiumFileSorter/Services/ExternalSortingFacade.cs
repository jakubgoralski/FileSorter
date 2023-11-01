using AltiumFileSorter.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi.Jwt;
using NickStrupat;

namespace AltiumFileSorter.Services
{
    internal class ExternalSortingFacade
    {
        private Uri FilePath { get; set; }
        private Uri ChunksLocation { get; set; }
        private int NumberOfChunks = 0;

        private readonly ulong MinimalEmptySpace = 100 * 1_048_576;

        public ExternalSortingFacade(string filePath)
        {
            FilePath = new Uri(filePath);
            ChunksLocation = new Uri(Path.Combine(Path.GetDirectoryName(filePath), "chunks"));
            if (!Directory.Exists(ChunksLocation.AbsolutePath))
            {
                Directory.CreateDirectory(ChunksLocation.AbsolutePath);
            }
        }

        public void Run()
        {
            ReadAndSortAndSaveChunks();
            MergeAllChunks();
        }

        private void ReadAndSortAndSaveChunks()
        {
            List<Row> chunk = new List<Row>();
            ComputerInfo computerInfo = new ComputerInfo();
            ulong allFreeMemory = computerInfo.AvailablePhysicalMemory;

            int i = 0;
            foreach (string line in File.ReadLines(FilePath.AbsolutePath))
            {
                chunk.Add(new Row(line));

                //bool runsOutOfSpace = (allFreeMemory - (ulong)GC.GetTotalMemory(false)) <= MinimalEmptySpace;
                bool runsOutOfSpace = i++ == 10_000_000;
                if (runsOutOfSpace)
                {
                    i = 0;
                    chunk.Sort();
                    File.WriteAllLines($"{ChunksLocation.AbsolutePath}/{NumberOfChunks++}.txt", chunk.Select(x => x.ToString()));
                    chunk.Clear();
                }
            }

            if(chunk.Any())
            {
                chunk.Sort();
                File.WriteAllLines($"{ChunksLocation.AbsolutePath}/{NumberOfChunks++}.txt", chunk.Select(x => x.ToString()));
                chunk.Clear();
            }
        }

        private void MergeAllChunks()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(ChunksLocation.AbsolutePath);
            while (Directory.GetFiles(ChunksLocation.AbsolutePath, "*", SearchOption.TopDirectoryOnly).Length > 1)
            {
                string firstFile = Path.Combine(ChunksLocation.AbsolutePath, directoryInfo.GetFiles().Select(x => x.Name).ElementAt(0));
                string secondFile = Path.Combine(ChunksLocation.AbsolutePath, directoryInfo.GetFiles().Select(x => x.Name).ElementAt(1));

                IEnumerable<Row> firstChunk = File.ReadLines(firstFile).Select(x => new Row(x));
                IEnumerable<Row> secondChunk = File.ReadLines(secondFile).Select(x => new Row(x));
                IEnumerable<Row> result = Merge(firstChunk.GetEnumerator(), secondChunk.GetEnumerator());

                using (StreamWriter writetext = new StreamWriter(Path.Combine(ChunksLocation.AbsolutePath, $"{NumberOfChunks++}.txt")))
                {
                    foreach (Row line in result)
                    {
                        writetext.WriteLine(line.ToString());
                    }
                }

                File.Delete(firstFile);
                File.Delete(secondFile);
            }
        }
        private IEnumerable<Row> Merge(IEnumerator<Row> firstEnumerator, IEnumerator<Row> secondEnumerator)
        {
            bool firstExists = false;
            bool secondExists = false;
            bool blockFirstMoveNext = false;
            bool blockSecondMoveNext = false;
            do
            {
                if (!blockFirstMoveNext)
                    firstExists = firstEnumerator.MoveNext();
                else
                    blockFirstMoveNext = false;

                if (!blockSecondMoveNext)
                    secondExists = secondEnumerator.MoveNext();
                else
                    blockSecondMoveNext = false;

                if (!firstExists && !secondExists)
                    break;

                if (firstExists && secondExists)
                {
                    if (firstEnumerator.Current.CompareTo(secondEnumerator.Current) == -1)
                    {
                        yield return firstEnumerator.Current;
                        blockSecondMoveNext = true;
                    }
                    else
                    {
                        yield return secondEnumerator.Current;
                        blockFirstMoveNext = true;
                    }
                }
                else if (firstExists)
                {
                    yield return firstEnumerator.Current;
                }
                else if (secondExists)
                {
                    yield return secondEnumerator.Current;
                }
            } while (true);
        }
    }
}
