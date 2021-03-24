class UserModel {
    constructor(){
        this.id;
        this.accessFailedCount;
        this.concurrencyStamp;
        this.email;
        this.emailConfirmed;
        this.firstName;
        this.lastName;
        this.lockoutEnabled;
        this.lockoutEnd;
        this.normalizedEmail;
        this.normalizedUserName;
        this.passwordHash;
        this.phoneNumber;
        this.phoneNumberConfirmed;
        this.securityStamp;
        this.twoFactorEnabled;
        this.userName;
    }
}

class AccountModel {
    constructor()
    {
        this.Id;
        this.Name;
        this.Url;
        this.Login;
        this.Password;
        this.StorageName;
    }
}

// class Passport {
//     constructor()
//     {
//         this.nationality;
//         this.number;
//         this.fullName;
//         this.gender;
//         this.birth;
//         this.placeIssue;
//         this.dateIssue;
//         this.expirationDate;
//     }
// }

class ChangeKeyStorageModel {
    constructor()
    {
        this.Name;
        this.OldKey;
        this.NewKey;
        this.ConfirmKey;
    }
}

class StorageModel {
    constructor()
    {
        this.Id = 0;
        this.Name;
        this.CountAccaunts = 0
        this.CountDocuments = 0;
        this.CountImages = 0;
        this.CountAudio = 0;
    }
}

class StorageRequestModel {
    constructor()
    {
        this.Storage = new StorageModel();
        this.Key;
    }
}

class FileModel {
    constructor()
    {
        this.Id;
        this.Name;
        this.Size;
        this.Type;
        this.Path;
    }
}

class ChangePasswordModel {
    constructor()
    {
        this.OldPassword;
        this.NewPassword;
        this.ConfirmPassword;
        this.StatusMessage;
    }
}

class ChangeEmailModel {
    constructor()
    {
        this.Username;
        this.IsEmailConfirmed;
        this.Email;
        this.StatusMessage;
    }
}

class LoginModel {
    constructor()
    {
        this.UserName;
        this.Password;
        this.RememberMe;
    }
}

class RegisterModel {
    constructor()
    {
        this.UserName;
        this.Email;
        this.Password;
        this.ConfirmPassword;
    }
}

class ResetPasswordModel {
    constructor()
    {
        this.Email;
        this.Password;
        this.ConfirmPassword;
        this.Code;
    }
}

class ForgotPassword {
    constructor()
    {
        this.Email;
    }
}

class ConfirmModel {
    constructor()
    {
        this.Password;
    }
}

class KeyModel {
    constructor()
    {
        this.StorageName;
        this.Key;
    }
}

class ResponseModel {
    constructor()
    {
        this.data;
        this.status;
    }
}

class UploadFileModel {
    constructor()
    {
        this.Name;
        this.FormData;
    }
}