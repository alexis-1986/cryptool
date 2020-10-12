# Cryptool
## Preview
A c# winForm application to encrypt a list of credentials. The logins, passwords and comments can only be seen and modified from the HMI. Once the user is done and saves his data the application creates 2 files:
  * A data file (data.txt) containing the "encrypted" credentials
  * A key file (key.txt) containing the key used for encryption and required for decryption
 
 To retrieve the information the user needs the application and the 2 files.
 The encryption algorithm is not suitable for industrial !!

![Crypt](https://user-images.githubusercontent.com/65492080/95744849-5541c980-0c94-11eb-8e73-606b96e7fea2.PNG)

## Usage
Save credentials:
  * Fill the table with your logins, passwords and comments
  * Click on the "Save" button

Retrieve credentials:
  * Start the application: default data and key files will be loaded
  Or
  * Click on the "Settings" button
  * From the "Settings" menu click on "Load data file" to select a data file
  * From the "Settings" menu click on "Load key file" to select a key file
  * Click on the "Reload" button to refresh the table

## Installation
This project was built using Visual Studio 2017.

