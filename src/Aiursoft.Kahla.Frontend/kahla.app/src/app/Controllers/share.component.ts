import { Component, DoCheck, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Values } from '../values';
import { MessageService } from '../Services/MessageService';
import { CacheService } from '../Services/CacheService';
import { UserSearchResult } from '../Models/Search/UserSearchResult';
import { MessageFileRef } from '../Models/MessageFileRef';

@Component({
    templateUrl: '../Views/share.html',
    styleUrls: [
        '../Styles/menu.scss',
        '../Styles/search-part.scss',
        '../Styles/friends.scss',
        '../Styles/button.scss',
        '../Styles/badge.scss',
    ],
    standalone: false,
})
export class ShareComponent implements OnInit, DoCheck {
    public loadingImgURL = Values.loadingImgURL;
    public showUsers = true;
    public message: string;
    public fileRef: MessageFileRef;
    public relativePath: boolean;
    public srcConversation: number;
    public inApp = false;
    public results: UserSearchResult;
    public searchTxt = '';

    constructor(
        private router: Router,
        private route: ActivatedRoute,
        private messageService: MessageService,
        public cacheService: CacheService
    ) {}

    public ngOnInit(): void {
        this.route.params.subscribe(param => {
            if (param.message) {
                this.message = param.message;
                this.inApp = true;
            } else if (param.srcConversation) {
                this.srcConversation = param.srcConversation;
                // this.fileRef = this.messageService.shareRef;
                // this.messageService.shareRef = null;
                this.relativePath = !!param.relativePath;
                this.inApp = true;
            } else {
                const parsedUrl = new URL(location.href);
                const text = parsedUrl.searchParams.get('text');
                const url = parsedUrl.searchParams.get('url');
                this.message = `${text ?? ''} ${url ?? ''}`;
            }
        });
        this.search('');
    }

    public showUsersResults(selectUsers: boolean): void {
        this.showUsers = selectUsers;
    }

    public share(user: unknown, group: boolean): void {
        // let conversationID = group ? (<GroupsResult>user).id : 0;
        // if (!group) {
        //     this.friendsApiService
        //         .UserDetail((<KahlaUser>user).id)
        //         .subscribe((result) => {
        //             if (result.code === 0) {
        //                 conversationID = result.conversationId;
        //             } else {
        //                 return;
        //             }
        //         });
        // }
        // const name = group
        //     ? (<GroupsResult>user).name
        //     : (<KahlaUser>user).nickName;
        // const preConfirm = (input) => {
        //     // get conversation detail
        //     return new Promise<void>(async (resolve) => {
        //         if (
        //             this.messageService.conversation &&
        //             this.messageService.conversation.id === conversationID
        //         ) {
        //             this.conversation = this.messageService.conversation;
        //         } else {
        //             this.conversation = (
        //                 await this.conversationApiService
        //                     .ConversationDetail(conversationID)
        //                     .toPromise()
        //             ).value;
        //         }
        //         if (this.fileRef) {
        //             // get file path
        //             const filePath = this.relativePath
        //                 ? this.fileRef.filePath
        //                 : this.fileRef.filePath.match(
        //                       new RegExp(
        //                           `.+\/conversation-${this.srcConversation}\/(.+)`
        //                       )
        //                   )?.[1];
        //             if (!filePath) {
        //                 resolve();
        //                 Swal.fire(
        //                     "Failed.",
        //                     "Sorry, but you can't share this file.",
        //                     "error"
        //                 );
        //                 return;
        //             }
        //             // copy file
        //             this.filesApiService
        //                 .ForwardMedia(
        //                     this.srcConversation,
        //                     filePath,
        //                     conversationID
        //                 )
        //                 .subscribe(async (t) => {
        //                     // build message with new file path
        //                     const targetFileRef = Object.assign(
        //                         {},
        //                         this.fileRef
        //                     );
        //                     targetFileRef.filePath = t.filePath;
        //                     await this.uploadService.encryptThenSend(
        //                         targetFileRef,
        //                         conversationID
        //                     );
        //                     resolve();
        //                     history.back();
        //                     setTimeout(
        //                         () =>
        //                             SwalToast.fire(
        //                                 "Send success",
        //                                 "",
        //                                 "success"
        //                             ),
        //                         100
        //                     );
        //                 });
        //         } else {
        //             this.sendMessage(
        //                 this.inApp ? this.message : input
        //             ).subscribe((result) => {
        //                 resolve();
        //                 if (result.code === 0) {
        //                     if (this.inApp) {
        //                         history.back();
        //                     } else {
        //                         this.router.navigate(["/home"], {
        //                             replaceUrl: true,
        //                         });
        //                     }
        //                     setTimeout(
        //                         () =>
        //                             SwalToast.fire(
        //                                 "Send success",
        //                                 "",
        //                                 "success"
        //                             ),
        //                         100
        //                     );
        //                 } else {
        //                     Swal.fire(
        //                         "Send failed",
        //                         "Something went wrong!",
        //                         "error"
        //                     );
        //                 }
        //             });
        //         }
        //     });
        // };
        // if (this.inApp) {
        //     const msgType = this.fileRef
        //         ? this.uploadService.getFileDescriptionFromType(
        //               this.fileRef.fileType
        //           )
        //         : this.cacheService.modifyMessage(this.message, true);
        //     Swal.fire({
        //         title: `Share ${msgType}?`,
        //         text: `Are you sure to send this ${msgType} to ${name}?`,
        //         showCancelButton: true,
        //         icon: "question",
        //         preConfirm: preConfirm,
        //         showLoaderOnConfirm: true,
        //     });
        // } else {
        //     Swal.fire({
        //         title: "Share message to",
        //         text: `Are you sure to send this message to ${name}?`,
        //         input: "textarea",
        //         inputValue: this.message,
        //         showCancelButton: true,
        //         preConfirm: preConfirm,
        //         showLoaderOnConfirm: true,
        //     });
        // }
    }

    public search(term: string, keydown = false): void {
        // if (this.cacheService.cachedData.contacts) {
        //     this.results = Object.assign(
        //         {},
        //         this.cacheService.cachedData.contacts
        //     );
        //     if (term) {
        //         this.results.users = this.results.users.filter((user) => {
        //             const regex = RegExp(term, "i");
        //             return (
        //                 regex.test(user.user.nickName) ||
        //                 (user.user.email && regex.test(user.email))
        //             );
        //         });
        //         this.results.threads = this.results.threads.filter((group) => {
        //             const regex = RegExp(term, "i");
        //             return regex.test(group.name);
        //         });
        //     }
        //     if (keydown) {
        //         if (
        //             this.showUsers &&
        //             this.results.users.length === 0 &&
        //             this.results.threads.length !== 0
        //         ) {
        //             this.showUsers = false;
        //         } else if (
        //             !this.showUsers &&
        //             this.results.threads.length === 0 &&
        //             this.results.users.length !== 0
        //         ) {
        //             this.showUsers = true;
        //         }
        //     }
        // }
    }

    public goSingleSearch(): void {
        setTimeout(() => {
            // if (this.showUsers) {
            //     if (this.results.users.length === 1) {
            //         this.share(this.results.users[0].user, false);
            //     }
            // } else {
            //     if (this.results.threads.length === 1) {
            //         this.share(this.results.threads[0], true);
            //     }
            // }
        }, 0);
    }

    ngDoCheck(): void {
        this.search(this.searchTxt);
    }
}
