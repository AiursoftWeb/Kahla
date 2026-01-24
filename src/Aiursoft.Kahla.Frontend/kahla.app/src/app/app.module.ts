// Modules
import { AppRoutingModule } from './Modules/AppRoutingModule';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
// Component
import { AppComponent } from './Controllers/app.component';
import { ConversationsComponent } from './Controllers/conversations.component';
import { FriendsComponent } from './Controllers/friends.component';
import { SearchServerComponent } from './Controllers/search-server.component';
import { SettingsComponent } from './Controllers/settings.component';
import { TalkingComponent } from './Controllers/talking.component';
import { SignInComponent } from './Controllers/signin.component';
import { NavComponent } from './Controllers/nav.component';
import { HeaderComponent } from './Controllers/header.component';
import { UserComponent } from './Controllers/user.component';
import { AboutComponent } from './Controllers/about.component';
import { UserDetailComponent } from './Controllers/userDetail.component';
import { ChangePasswordComponent } from './Controllers/changePassword.component';
import { DevicesComponent } from './Controllers/devices.component';
import { ThemeComponent } from './Controllers/theme.component';
import { AdvancedSettingComponent } from './Controllers/advanced-setting.component';
import { HomeComponent } from './Controllers/home.component';
import { ShareComponent } from './Controllers/share.component';
import { FileHistoryComponent } from './Controllers/file-history.component';
// Services
import { ApiService } from './Services/Api/ApiService';
import { CacheService } from './Services/CacheService';
import { UploadService } from './Services/UploadService';
import { AuthApiService } from './Services/Api/AuthApiService';
import { FilesApiService } from './Services/Api/FilesApiService';
import { MessageService } from './Services/MessageService';
import { InitService } from './Services/InitService';
import { DevicesApiService } from './Services/Api/DevicesApiService';
import { ThemeService } from './Services/ThemeService';
import { HomeService } from './Services/HomeService';
import { ProbeService } from './Services/ProbeService';
import { EventService } from './Services/EventService';
import { GlobalNotifyService } from './Services/GlobalNotifyService';
import { MessagesApiService } from './Services/Api/MessagesApiService';
import { ContactsApiService } from './Services/Api/ContactsApiService';
import { SearchApiService } from './Services/Api/SearchApiService';
import { MyContactsRepository } from './Repositories/MyContactsRepository';
import { ContactListComponent } from './Controllers/contact-list.component';
import { ThreadsListComponent } from './Controllers/threads-list.component';
import { MyThreadsRepository } from './Repositories/ThreadsRepository';
import { ThreadsApiService } from './Services/Api/ThreadsApiService';
import { SearchAreaComponent } from './Controllers/search-area.component';
import { SearchTypeComponent } from './Controllers/search-type.component';
import { BlocksApiService } from './Services/Api/BlocksApiService';
import { BlocksListComponent } from './Controllers/blocks-list.component';
import { TruncatedNumPipe } from './Pipes/truncated-num.pipe';
import { ThreadOptionsComponent } from './Controllers/thread-options.component';
import { ToggleMenuItemComponent } from './Controllers/toggle-menu-item.component';
import { NewThreadComponent } from './Controllers/new-thread.component';
import { IconForFilePipe } from './Pipes/icon-for-file.pipe';
import { ThreadAvatarComponent } from './Controllers/thread-avatar.component';
import { MessageSegmentTextComponent } from './Controllers/MessageSegments/mseg-text.component';
import { MessageSegmentImgComponent } from './Controllers/MessageSegments/mseg-img.component';
import { MessageSegmentVideoComponent } from './Controllers/MessageSegments/mseg-video.component';
import { MessageSegmentVoiceComponent } from './Controllers/MessageSegments/mseg-voice.component';
import { MessageSegmentFileComponent } from './Controllers/MessageSegments/mseg-file.component';
import { MessageComponent } from './Controllers/message.component';
import { HumanReadableSizePipe } from './Pipes/human-readable-size.pipe';
import { StorageUrlPipe } from './Pipes/storage-url.pipe';
import { ScrollButtonComponent } from './Controllers/scroll-button.component';
import { FileSharingButtonsComponent } from './Controllers/MessageSegments/file-sharing-buttons.component';
import { ThreadInfoComponent } from './Controllers/thread-info.component';
import { ThreadMembersComponent } from './Controllers/thread-members.component';
import { LoadMoreButtonComponent } from './Controllers/load-more-button.component';
import { MessageListComponent } from './Controllers/message-list.component';
import { TalkingInputComponent } from './Controllers/talking-input.component';
import { UserInfoCacheDictionary } from './Caching/UserInfoCacheDictionary';
import { AutofocusDirective } from './Directives/AutoFocusDirective';
import { ThreadNamePipe } from './Pipes/thread-name.pipe';
import { ThreadInfoCacheDictionary } from './Caching/ThreadInfoCacheDictionary';
import { ManageThreadComponent } from './Controllers/manage-thread.component';
import { EditableMenuItemComponent } from './Controllers/editable-menu-item.component';
import { MyThreadsOrderedRepository } from './Repositories/MyThreadsOrderedRepository';
import { ThreadInfoPublicComponent } from './Controllers/thread-info-public.component';
import { ThreadBasicInfoComponent } from './Controllers/thread-basic-info.component';
import { UserBasicInfoComponent } from './Controllers/user-basic-info.component';
import { InviteMemberComponent } from './Controllers/invite-member.component';
import { ThreadInfoPublicCacheDictionary } from './Caching/ThreadInfoPublicCacheDictionary';
import { CdkMenuModule } from '@angular/cdk/menu';
import { DropFileDirective } from './Directives/DropFileDirective';
import { AutoEncodeLinkPipe } from './Pipes/auto-encode-links.pipe';
import { MatchHeightForDirective } from './Directives/MatchHeightForDirective';
import { Logger } from './Services/Logger';
import { PlatformModule } from '@angular/cdk/platform';
import { SlateModule } from 'slate-angular';
import { SlateEditorComponent } from './Components/SlateEditor/slate-editor.component';
import { MentionElementComponent } from './Components/SlateEditor/mention-element.component';

@NgModule({
    imports: [
        BrowserModule,
        FormsModule,
        HttpClientModule,
        AppRoutingModule,
        TruncatedNumPipe,
        IconForFilePipe,
        HumanReadableSizePipe,
        StorageUrlPipe,
        ReactiveFormsModule,
        AutofocusDirective,
        ThreadNamePipe,
        MessageSegmentVideoComponent,
        FileSharingButtonsComponent,
        CdkMenuModule,
        DropFileDirective,
        AutoEncodeLinkPipe,
        MatchHeightForDirective,
        SlateModule,
    ],
    declarations: [
        AboutComponent,
        AppComponent,
        ConversationsComponent,
        FriendsComponent,
        SearchServerComponent,
        SettingsComponent,
        TalkingComponent,
        SignInComponent,
        NavComponent,
        HeaderComponent,
        UserComponent,
        UserDetailComponent,
        ThreadInfoComponent,
        ThreadMembersComponent,
        ChangePasswordComponent,
        DevicesComponent,
        ThemeComponent,
        AdvancedSettingComponent,
        HomeComponent,
        ShareComponent,
        FileHistoryComponent,
        ContactListComponent,
        ThreadsListComponent,
        SearchAreaComponent,
        SearchTypeComponent,
        BlocksListComponent,
        ThreadOptionsComponent,
        ToggleMenuItemComponent,
        NewThreadComponent,
        ThreadAvatarComponent,
        TalkingInputComponent,
        MessageListComponent,
        MessageComponent,
        MessageSegmentTextComponent,
        MessageSegmentImgComponent,
        MessageSegmentVoiceComponent,
        MessageSegmentFileComponent,
        ScrollButtonComponent,
        LoadMoreButtonComponent,
        ManageThreadComponent,
        EditableMenuItemComponent,
        ThreadInfoPublicComponent,
        ThreadBasicInfoComponent,
        UserBasicInfoComponent,
        InviteMemberComponent,
        SlateEditorComponent,
        MentionElementComponent,
    ],
    providers: [
        ApiService,
        CacheService,
        UploadService,
        AuthApiService,
        MessagesApiService,
        FilesApiService,
        MessageService,
        InitService,
        DevicesApiService,
        ThemeService,
        HomeService,
        ProbeService,
        EventService,
        GlobalNotifyService,
        ContactsApiService,
        SearchApiService,
        MyContactsRepository,
        MyThreadsRepository,
        ThreadsApiService,
        BlocksApiService,
        UserInfoCacheDictionary,
        ThreadInfoCacheDictionary,
        MyThreadsOrderedRepository,
        ThreadInfoPublicCacheDictionary,
        Logger,
        PlatformModule,
    ],
    bootstrap: [AppComponent],
})
export class AppModule {}

// platformBrowserDynamic().bootstrapModule(AppModule);
