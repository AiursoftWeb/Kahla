import {
    Component,
    effect,
    ElementRef,
    input,
    model,
    output,
    signal,
    viewChild,
} from '@angular/core';
import { CacheService } from '../Services/CacheService';
import { MessageContent } from '../Models/Messages/MessageContent';
import type { EmojiButton } from '@joeattardi/emoji-button';
import { ThemeService } from '../Services/ThemeService';
import { VoiceRecorder } from '../Utils/VoiceRecord';
import { MessageSegmentText, MessageTextWithAnnotate } from '../Models/Messages/MessageSegments';
import { imageFileTypes, selectFiles } from '../Utils/SystemDialog';
import { SlateEditorComponent } from '../Components/SlateEditor/slate-editor.component';
import { KahlaUser } from '../Models/KahlaUser';
import { Logger } from '../Services/Logger';
import { debounceTime, distinctUntilChanged, filter, map, Subscription } from 'rxjs';
import { ThreadsApiService } from '../Services/Api/ThreadsApiService';
import { ThreadInfoJoined } from '../Models/Threads/ThreadInfo';
import { MessageTextAnnotatedMention } from '../Models/Messages/MessageTextAnnotated';
import { ThreadMembersRepository } from '../Repositories/ThreadMembersRepository';

export interface PendingFile {
    file: File;
    displayName: string;
    previewUrl?: string;
    type: 'image' | 'video' | 'file';
}

@Component({
    selector: 'app-talking-input',
    templateUrl: '../Views/talking-input.html',
    styleUrls: ['../Styles/talking-input.scss', '../Styles/button.scss', '../Styles/popups.scss'],
    standalone: false,
})
export class TalkingInputComponent {
    textContent = signal<MessageTextWithAnnotate[]>([]);
    showPanel = model(false);
    pendingFiles = signal<PendingFile[]>([]);
    sendMessage = output<{
        content: MessageContent;
        ats?: string[];
    }>();

    private picker: EmojiButton;
    private chatBox = viewChild.required<ElementRef<HTMLElement>>('chatBox');
    private chatInput = viewChild.required<SlateEditorComponent>('chatInput');

    atRecommends = signal<ThreadMembersRepository | null>(null);
    atRecommendsShowPos = signal<[number, number] | null>(null);
    readonly threadInfo = input<ThreadInfoJoined>();

    recorder = new VoiceRecorder(180);

    constructor(
        public cacheService: CacheService,
        private themeService: ThemeService,
        private threadApiService: ThreadsApiService,
        private logger: Logger
    ) {
        effect(cleanup => {
            if (this.threadInfo()?.allowMembersEnlistAllMembers || this.threadInfo()?.imAdmin) {
                const sub = new Subscription();
                const wordChanged = this.chatInput()
                    .lastInputWordChanged.pipe(
                        filter(t => (t?.word?.startsWith('@') && t.word.length <= 41) ?? false),
                        map(t => t.word!.slice(1).toLowerCase()),
                        distinctUntilChanged()
                    );

                sub.add(
                    wordChanged.subscribe(t => {
                        logger.debug('Update member info by word: ', t);
                        const repo = new ThreadMembersRepository(
                            this.threadApiService,
                            this.threadInfo()!.id,
                            t || undefined,
                            undefined,
                            this.threadInfo()?.topTenMembers?.filter(m =>
                                m.user.nickName.toLowerCase().includes(t)
                            )
                        );
                        this.atRecommends.set(repo);
                    })
                );

                sub.add(
                    wordChanged.pipe(debounceTime(200)).subscribe(() => {
                        void this.atRecommends()?.updateAll();
                    })
                );

                sub.add(
                    this.chatInput()
                        .lastInputWordChanged.pipe(distinctUntilChanged())
                        .subscribe(t => {
                            this.atRecommendsShowPos.set(
                                t.word?.startsWith('@') ? t.caretEndPos : null
                            );
                        })
                );
                cleanup(() => sub.unsubscribe());
            }
        });
    }

    public async emoji() {
        if (!this.picker) {
            const EmojiButton = (await import('@joeattardi/emoji-button')).EmojiButton;
            this.picker = new EmojiButton({
                position: 'top-start',
                zIndex: 20,
                theme: this.themeService.IsDarkTheme() ? 'dark' : 'light',
                autoFocusSearch: false,
                showSearch: false,
            });
            this.picker.on('emoji', emoji => {
                // eslint-disable-next-line @typescript-eslint/no-unsafe-member-access
                this.insertToSelection(emoji.emoji as string);
            });
        }
        this.picker.togglePicker(this.chatBox().nativeElement);
    }

    public insertToSelection(content: string) {
        this.chatInput().insertTextToCaret(content);
    }

    public async record() {
        await this.recorder.init();
        if (this.recorder.recording) {
            this.recorder.stopRecording();
        } else {
            this.recorder.startRecording();
        }
    }

    public togglePanel(): void {
        this.showPanel.set(!this.showPanel());
        setTimeout(() => {
            if (this.showPanel()) {
                window.scroll(0, window.scrollY + 105);
            } else {
                window.scroll(0, window.scrollY - 105);
            }
        }, 0);
    }

    public startInput(): void {
        if (this.showPanel()) {
            this.togglePanel();
        }
    }

    inputKeydown(e: KeyboardEvent) {
        if (e.key === 'Enter') {
            e.preventDefault();
            if (
                (e.altKey || e.ctrlKey || e.shiftKey) ===
                this.cacheService.mine()?.privateSettings?.enableEnterToSendMessage
            ) {
                this.insertToSelection('\n');
            } else {
                // send
                this.send();
            }
        }
    }

    public send() {
        if (this.textContent()?.length) {
            this.logger.debug('Constructing text message...', this.textContent());
            const ats = this.textContent()
                .filter(t => typeof t !== 'string' && t.annotated === 'mention')
                .map(t => (t as MessageTextAnnotatedMention).targetId);
            this.logger.debug('At users:', ats);
            this.sendMessage.emit({
                // TODO: consider use a factory to build this thing
                content: {
                    v: 1,
                    segments: [
                        {
                            type: 'text',
                            content: this.textContent(),
                        } satisfies MessageSegmentText,
                    ],
                },
                ats: ats.length ? ats : undefined,
            });
            this.chatInput().clear();
        }
    }

    fileDropped(items: [File, string][]) {
        const newPendingFiles = items.map(([file]) => {
            const type = this.detectFileType(file);
            return this.createPendingFile(file, type);
        });
        this.pendingFiles.update(files => [...files, ...newPendingFiles]);
    }

    private detectFileType(file: File): 'img' | 'video' | 'file' {
        if (file.type.startsWith('image/')) {
            return 'img';
        } else if (file.type.startsWith('video/')) {
            return 'video';
        }
        return 'file';
    }

    async selectFile(type: 'img' | 'video' | 'file') {
        let accept: string[] | '*/*' = '*/*';
        switch (type) {
            case 'img':
                accept = imageFileTypes;
                break;
            case 'video':
                accept = ['video/mp4', 'video/webm'];
                break;
        }

        const res = await selectFiles(true, accept);
        if (!res) return;

        const newPendingFiles = res.map(file => this.createPendingFile(file, type));
        this.pendingFiles.update(files => [...files, ...newPendingFiles]);
    }

    private createPendingFile(file: File, type: 'img' | 'video' | 'file'): PendingFile {
        const fileType: PendingFile['type'] = type === 'img' ? 'image' : type === 'video' ? 'video' : 'file';
        let displayName: string;
        let previewUrl: string | undefined;

        if (fileType === 'image') {
            // For images, generate privacy-safe name: img_timestamp.extension
            const extension = file.name.split('.').pop() ?? 'png';
            displayName = `img_${Date.now()}.${extension}`;
            previewUrl = URL.createObjectURL(file);
        } else {
            // For videos and files, keep original name
            displayName = file.name;
        }

        return {
            file,
            displayName,
            previewUrl,
            type: fileType,
        };
    }

    public removeFile(index: number) {
        const files = this.pendingFiles();
        const fileToRemove = files[index];
        if (fileToRemove?.previewUrl) {
            URL.revokeObjectURL(fileToRemove.previewUrl);
        }
        this.pendingFiles.update(f => f.filter((_, i) => i !== index));
    }

    public mention(targetUser: KahlaUser) {
        this.chatInput().insertMentionToCaret(targetUser);

        // Focus the input after selecting a user
        this.chatInput().focus();
    }

    public completeMentionMenu(targetUser: KahlaUser) {
        // remove the typing partial mention text
        this.chatInput().removeTextFromCursorTill('@');
        this.atRecommendsShowPos.set(null);
        this.mention(targetUser);
    }
}
