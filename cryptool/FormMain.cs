using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace cryptool
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            // to prevent resize
            FormBorderStyle = FormBorderStyle.FixedSingle;

            InitializeComponent();
        }

        // defautl path for data and key files
        string pathToDataFile = @"data.txt";
        string pathToKeyFile = @"key.txt";

        // binding source for the data grid view
        BindingSource bds = new BindingSource();

        /// <summary>
        /// helper method to byte array to string
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private string bytesToString(byte [] bytes)
        {
            StringBuilder sb = new StringBuilder();

            foreach (byte b in bytes)
            {
                sb.AppendFormat("{0:X2}", b);
            }

            return sb.ToString();
        }

        /// <summary>
        ///  helper method to convert string to byte array
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private byte[] stringToBytes(string str)
        {
            // the number of characters should be 
            // even to have a round bytes number
            if ((str.Length % 2) != 0)
            {
                return null;
            }

            // 2 caracters = 1 byte
            int byteLength = str.Length / 2;
            Byte[] byteRes = new byte[byteLength]; 

            // Loop through the string caracters 2 by 2
            for (int i = 0; i < str.Length - 1; i += 2)
            {
                byteRes[i / 2] = Convert.ToByte(str.Substring(i, 2), 16);
            }         
            
            return byteRes;
        }

        /// <summary>
        /// method to update the data grid view
        /// from the binding source
        /// </summary>
        private void updateDataGridView()
        {
            // clear the binding source
            bds.Clear();

            // error message content
            string message = string.Empty;

            // check if the path to the data file exists
            if (File.Exists(pathToDataFile))
            {
                // check if the path to the key file exists
                if (File.Exists(pathToKeyFile))
                {
                    // retrieve the key from the key file
                    byte[] key = readKeyFromTxt(pathToKeyFile);

                    // if the key is not null
                    if (key != null)
                    {
                        // load the data from the data file using the key
                        loadDataFromTxt(pathToDataFile, key, bds);

                        // if no data is retrieve
                        if (bds.Count == 0)
                        {
                            // all the files are present but decryption is not Ok
                            message = "No data decrypted: data and key may be inconsistent";                           
                        }
                    }
                    else
                    {
                        // key file is present but its content is null or empty
                        message = "No data decrypted: check the key file content";                      
                    }
                }
                else
                {
                    // key file is not present
                    message = "No data decrypted: check the key file path";                  
                }
            }
            else
            {
                // data file is not present
                message = "No data decrypted: check the data file path";             
            }

            // if the error message has been filled
            // display it in a message box
            if (message != string.Empty)
            {
                // add an empty entry otherwise the add line isn't 
                // available form the datagridview
                bds.Add(new Entry { Login = string.Empty, Password = string.Empty, Comments = string.Empty });
                MessageBox.Show(message);
            }         
        }

        /// <summary>
        /// method to load the form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormMainLoad(object sender, EventArgs e)
        {    
            // set the datagridview data source
            dgvEntries.DataSource = bds;
            // update the datagridview
            updateDataGridView(); 
        }

        /// <summary>
        /// method to encrypt a byte array using a key
        /// </summary>
        /// <param name="clear">byte array to encrypt</param>
        /// <param name="key">key to use for encryption</param>
        /// <returns></returns>
        private byte[] encryptFromBytesWithKey(byte[] clear, byte[] key)
        {
            // calculate the byte array and the key size
            int byteSize = clear.Length;
            int keySize = key.Length;

            // byte array to store the encrypted bytes
            byte[] encrypted = new byte[byteSize];

            // loop through the byte array
            // !!! byte array and key can have different size
            // !!! index value must be checked accordingly
            for (int i = 0; i < byteSize && i < keySize; ++i)
            {
                // encryption is a simple XOR between the byte array and the key
                // XOR result is an integer which must be converted to a byte
                encrypted[i] = Convert.ToByte(clear[i] ^ key[i]);
            }

            return encrypted;
        }

        /// <summary>
        /// method to deserialize an entry object from
        /// a byte array
        /// </summary>
        /// <param name="entry">byte array to deserialize</param>
        /// <returns></returns>
        private Entry deserializeFromBytes(byte[] entry)
        {
            Entry deserializeEntry = null;

            using(MemoryStream ms = new MemoryStream(entry))
            {
                try
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    deserializeEntry = (bf.Deserialize(ms) as Entry);
                }
                catch(Exception ex)
                {
                    Debug.WriteLine(string.Format("deserializeFromByte error: {0}", ex.Message));            
                }             
            }

            return deserializeEntry;
        }

        /// <summary>
        /// method to read a key as a byte array from
        /// a txt file
        /// </summary>
        /// <param name="pathToKeyFile"></param>
        /// <returns></returns>
        private byte[] readKeyFromTxt(string pathToKeyFile)
        {
            // read all the lines from the txt file
            List<byte[]> bytes = readBytesFromTxt(pathToKeyFile);

            // if the file is not empty the key is the 
            // first line
            if (bytes.Count > 0)
            {
               return bytes[0];
            }    
            else
            {
                return null;
            }
        }

        /// <summary>
        /// method used to serialize an entry object to a 
        /// byte array
        /// </summary>
        /// <param name="entry">entry object to serialize</param>
        /// <returns></returns>
        private byte[] serializeFromEntry(Entry entry)
        {
            byte[] serialized = null;

            using(MemoryStream ms = new MemoryStream())
            {
                try
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(ms, entry);
                    serialized = ms.ToArray();
                }
                catch(Exception ex)
                {
                    Debug.WriteLine(string.Format("serializeFromEntry error: {0}", ex.Message));
                }            
            }

            return serialized;
        }

        /// <summary>
        /// method used to generate a key with a specific size
        /// </summary>
        /// <param name="size">desired key size</param>
        /// <returns></returns>
        private byte[] generateKey(int size)
        {
            byte[] key = new byte[size];

            //RNGCryptoServiceProvider is an implementation of a random number generator.
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            rng.GetBytes(key);

            return key;
        }

        /// <summary>
        /// method used to calculate the key size
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private int calculateKeySize(List<byte[]> bytes)
        {
            // key size is equal to the length of the largest byte array of the list
            return bytes.Max(b => b.Length);
        }

        /// <summary>
        /// method used to read bytes on several lines from
        /// a txt file
        /// </summary>
        /// <param name="pathToFile">the path to the txt file to read</param>
        /// <returns></returns>
        private List<byte[]> readBytesFromTxt(string pathToFile)
        {
            string line = string.Empty;
            List<byte[]> bytes = new List<byte[]>();

            try
            {
                using (StreamReader sr = new StreamReader(pathToFile))
                {
                    // read all lines one by one
                    while ((line = sr.ReadLine()) != null)
                    {
                        // convert the string representation like "1F03..." to bytes
                        // and add it to the bytes list
                        bytes.Add(stringToBytes(line));
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(string.Format("readBytesFromTxt error: {0}", ex.Message));
            }
           
            return bytes;
        }

        /// <summary>
        /// method used to write a list of bytes to a txt file
        /// </summary>
        /// <param name="pathToFile">path to the txt file</param>
        /// <param name="bytes">the list of bytes to write</param>
        private void writeBytesToTxt(string pathToFile, List<byte[]> bytes)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(pathToFile))
                {
                    foreach (byte[] b in bytes)
                    {
                        // bytes are converted to a string representation
                        // and write to the file
                        sw.WriteLine(bytesToString(b));
                        // TO TRY: use the BitConverter object
                        // sw.WriteLine(BitConverter.ToString(b));
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("writeBytesToTxt error: {0}", ex.Message));
            }          
        }

        /// <summary>
        /// method to load the encrypted data from a txt file
        /// </summary>
        /// <param name="pathToFile"></param>
        /// <param name="key"></param>
        /// <param name="bds"></param>
        private void loadDataFromTxt(string pathToFile, byte[] key, BindingSource bds)
        {
            // read all the bytes from the data file
            List<byte[]> raw = readBytesFromTxt(pathToFile);
            
            // loop through the encrypted bytes
            for (int i = 0; i < raw.Count; ++i)
            {
                Debug.WriteLine("read before decrypt: " + bytesToString(raw[i]));

                // decrypt the bytes using the key parameter
                raw[i] = encryptFromBytesWithKey(raw[i], key);

                // try to deserialize the bytes to an enty object
                Entry e = deserializeFromBytes(raw[i]);

                // if the entry object is valid add it
                // to the binding source
                // !!! only valid object must be added added to the bs
                // !!! otherwise the datagridview doesn't load
                if (e != null)
                {
                    bds.Add(e);
                }                         
            }
        }

        /// <summary>
        /// method to save crypted entry objects to
        /// a txt file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSaveClick(object sender, EventArgs e)
        {
            // the list of bytes to be saved
            List<byte[]> bytes = new List<byte[]>();

            // loop through the entry objects in the binding source
            foreach (Entry entry in bds)
            {
                // serialize the entry object to bytes
                byte[] entryBytes = serializeFromEntry(entry);
                // add it to the bytes list
                bytes.Add(entryBytes);
                Debug.WriteLine(string.Format("encrypted: {0} size {1}", bytesToString(entryBytes), entryBytes.Length));
            }

            // generate a new key according to bytes size
            // !!! the key size must equal to the largest serialized entry object
            byte[] key = generateKey(calculateKeySize(bytes));
            
            // loop through the bytes list
            for (int i = 0; i < bytes.Count; ++i)
            {
                // and encrypt the bytes with the new key
                bytes[i] = encryptFromBytesWithKey(bytes[i], key);
            }

            // save the key to the key file in txt format
            writeBytesToTxt(pathToKeyFile, new List<byte[]> { key });
            // save the data to the data file in txt format
            writeBytesToTxt(pathToDataFile, bytes);
        }

        /// <summary>
        /// method to display a file dialog box to select
        /// a file
        /// </summary>
        /// <param name="title">dialog box title</param>
        /// <returns></returns>
        private string selectFile(string title)
        {
            string file = string.Empty;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;
                openFileDialog.Title = title;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Get the path of specified file
                    file = openFileDialog.FileName;
                }
            }
            // return the selected file
            return file;
        }

        /// <summary>
        /// "select data file " menu item click event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuDataFileClick(object sender, EventArgs e)
        {
            pathToDataFile = selectFile(@"Select data file");
        }

        /// <summary>
        /// "select key file" menu item click event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuKeyFileClisk(object sender, EventArgs e)
        {
            pathToKeyFile = selectFile(@"Select key file");
        }

        /// <summary>
        /// "reload" button click event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReloadClick(object sender, EventArgs e)
        {
            // update the datagridview
            updateDataGridView();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show(AppDomain.CurrentDomain.BaseDirectory);
        }
    }
}
