using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LibraryWithAlgorithms {
    public class LRU {
        private List<string> listOfLists { get; set; }
        int lenOfstr;
        private const char SPACE = ' ';

        public LRU(int buffer) {
            this.listOfLists = new List<string>();
            this.lenOfstr = buffer * 2 + 3;
        }

        public List<string> GetSteps() {
            return listOfLists;
        }

        private static List<int> LRUChange(List<int> block, int num) {
            List<int> res = new List<int>();
            for (int i = 0; i < block.Count; i++) {
                if (block[i] != num) {
                    res.Add(block[i]);
                }
            }
            res.Add(num);
            return res;
        }

        private static List<int> FIFOChange(List<int> block, int num) {
            for (int i = 1; i < block.Count; i++) {
                block[i - 1] = block[i];
            }
            block[block.Count - 1] = num;
            return block;
        }

        private List<int> FillFirst(List<int> input, int numOfFilled) {
            List<int> res = new List<int>();
            string list = "";

            for (int i = 0; i < numOfFilled; i++) {
                if (res.Contains(input[i])) {
                    res = LRUChange(res, input[i]);
                } else {
                    res.Add(input[i]);
                    list += input[i].ToString();
                    list += SPACE;
                }
                list = ResultToString(list, res);
                list = AddSpaces(list);
                this.listOfLists.Add(list);
                list = "";

            }
            return res;
        }

        private string AddSpaces(string str) {
            while (str.Length != lenOfstr) {
                str += SPACE;
            }
            return str;
        }

        private string ResultToString(string str, List<int> res) {
            for (int j = 0; j < res.Count; j++) {
                str += res[j].ToString();
                str += SPACE;
            }
            return str;
        }

        public int LRUAlgorithm(List<int> input, int buffer, int numOfFilled) {
            List<int> res = new List<int>();
            string list = "";
            int interrupts = 0;
            bool isInter = false;
            if (numOfFilled != 0) {
                res = FillFirst(input, numOfFilled);
            }
            if (input.Count == buffer) {
                return interrupts;
            }
            int count = numOfFilled;
            do {
                if (res.Contains(input[count])) {
                    res = LRUChange(res, input[count]);
                } else {
                    res.Add(input[count]);
                    interrupts++;
                    isInter = true;
                }
                list += input[count].ToString();
                list += SPACE;
                list = ResultToString(list, res);
                if (isInter) {
                    while (list.Length != lenOfstr - 1) {
                        list += SPACE;
                    }
                    list += "*";
                } else {
                    list = AddSpaces(list);
                }
                this.listOfLists.Add(list);
                list = "";
                count++;
                isInter = false;
                if (count == input.Count) {
                    return interrupts;
                }
            } while (res.Count != buffer);

            for (int i = count; i < input.Count; i++) {
                if (res.Contains(input[i])) {
                    res = LRUChange(res, input[i]);
                } else {
                    res = FIFOChange(res, input[i]);
                    interrupts++;
                    isInter = true;
                }
                list += input[count].ToString();
                list += SPACE;
                list = ResultToString(list, res);
                if (isInter) {
                    while (list.Length != lenOfstr - 1) {
                        list += SPACE;
                    }
                    list += "*";
                } else {
                    list = AddSpaces(list);
                }
                this.listOfLists.Add(list);
                list = "";
                count++;
                isInter = false;
            }

            return interrupts;
        }
    }
}
