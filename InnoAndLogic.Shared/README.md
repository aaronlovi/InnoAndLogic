# InnoAndLogic.Shared

## Overview
`InnoAndLogic.Shared` is a shared library designed to provide reusable components for .NET projects. It includes utilities, models, and enums that simplify common development tasks and promote code reuse across projects.

## Features
- **Encryption Utilities**: Securely encrypt and decrypt data using AES with password-based key derivation.
- **Result Handling**: Fluent APIs for handling operation results, including success and failure states.
- **Error Codes**: Predefined enums for consistent error handling.

## Usage
This library is published as a NuGet package and can be used in various projects, including `InnoAndLogic.Persistence` and other .NET applications.

### Installation
To install the NuGet package, use the following command:
dotnet add package InnoAndLogic.Shared
### Example Usage
#### Encryptionusing InnoAndLogic.Shared;

var password = "securePassword";
var salt = 12345678L;
var data = new byte[] { 1, 2, 3, 4 };

// Encrypt data
var encrypted = Encryption.Encrypt(password, salt, data);

// Decrypt data
var decrypted = Encryption.Decrypt(password, salt, encrypted);
#### Result Handlingusing InnoAndLogic.Shared;
using InnoAndLogic.Shared.Models;

var result = Result.Success;
result = result.Then(() => Result.Failure(ErrorCodes.ValidationError, "Invalid input"));

if (result.IsFailure) {
    Console.WriteLine(result.ErrorMessage);
}
## License
This library is licensed under the MIT License. See the LICENSE file for details.

## Repository
For more information, visit the [GitHub repository](https://github.com/aaronlovi/InnoAndLogic.Shared).