import { Component, input, output } from '@angular/core';
import { Router } from '@angular/router';
import { MessageService } from '../Services/MessageService';
import { HomeService } from '../Services/HomeService';
import { EventService } from '../Services/EventService';
import Swal from 'sweetalert2';

@Component({
    selector: 'app-header',
    templateUrl: '../Views/header.html',
    styleUrls: ['../Styles/header.scss', '../Styles/reddot.scss'],
    standalone: false,
})
export class HeaderComponent {
    public readonly title = input('Kahla');
    public readonly returnButton = input(true);
    public readonly returnButtonPreventDefault = input(false);
    public readonly closeDirectly = input(false);
    public readonly button = input(false);
    public readonly buttonLink = input<string>('');
    public readonly buttonIcon = input('');
    public readonly shadow = input(false);
    public readonly processing = input(false);

    public readonly returnButtonClicked = output();
    public readonly toolButtonClicked = output();

    constructor(
        private router: Router,
        public messageService: MessageService,
        public homeService: HomeService,
        public eventService: EventService
    ) {}

    public goBack(): void {
        this.returnButtonClicked.emit();
        if (this.returnButtonPreventDefault()) return;
        if (history.length === 1 || (this.homeService.wideScreenEnabled && this.closeDirectly())) {
            void this.router.navigate(['/home']);
        } else {
            history.back();
        }
    }

    public linkClicked() {
        this.toolButtonClicked.emit();
    }

    public async showDisconnectedDialog() {
        if (
            !(
                await Swal.fire({
                    title: 'Message Event Connection Down.',
                    text:
                        'This might because of the broken network environment.\n We will try to reconnect later, but before that,' +
                        ' your message might no be the latest.',
                    icon: 'warning',
                    confirmButtonText: 'Reconnect',
                    showCancelButton: true,
                })
            ).dismiss
        ) {
            this.eventService.attemptReconnect();
        }
    }
}
