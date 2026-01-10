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
import Swal from 'sweetalert2';
import { imageFileTypes, selectFiles } from '../Utils/SystemDialog';
import { MessageTextInputDirective } from '../Directives/MessageTextInputDirective';
import { KahlaUser } from '../Models/KahlaUser';
import { Logger } from '../Services/Logger';
import { debounceTime, distinctUntilChanged, filter, map, Subscription } from 'rxjs';
import { ThreadsApiService } from '../Services/Api/ThreadsApiService';
import { ThreadInfoJoined } from '../Models/Threads/ThreadInfo';
import { MessageTextAnnotatedMention } from '../Models/Messages/MessageTextAnnotated';
import { ThreadMembersRepository } from '../Repositories/ThreadMembersRepository';

@Component({
    selector: 'app-talking-input',
    templateUrl: '../Views/talking-input.html',
    styleUrls: ['../Styles/talking-input.scss', '../Styles/button.scss', '../Styles/popups.scss'],
    standalone: false,
})
export class TalkingInputComponent {
    textContent = signal<MessageTextWithAnnotate[]>([]);
    showPanel = model(false);
    sendMessage = output<{
        content: MessageContent;
        ats?: string[];
    }>();

    private picker: EmojiButton;
    private chatBox = viewChild.required<ElementRef<HTMLElement>>('chatBox');
    private chatInput = viewChild.required<MessageTextInputDirective>('chatInput');

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
            this.textContent.set([]);
            this.chatInput().forward();
        }
    }

    fileDropped(items: [File, string][]) {
        void Swal.fire({
            title: 'Dropped files',
            text: items.map(([, name]) => name).join('\n'),
        });
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
        console.log(res);
        void Swal.fire({
            title: 'Selected files',
            text: res.map(t => t.name).join('\n'),
        });
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
