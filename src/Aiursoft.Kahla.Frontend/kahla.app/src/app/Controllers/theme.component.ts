import { Component, OnInit } from '@angular/core';
import { Theme } from '../Models/Theme';
import { ThemeService } from '../Services/ThemeService';

@Component({
    templateUrl: '../Views/theme.html',
    styleUrls: ['../Styles/menu.scss', '../Styles/chip-button.scss'],
    standalone: false,
})
export class ThemeComponent implements OnInit {
    constructor(private themeService: ThemeService) {}

    public currentTheme: Theme = Theme.kahlaAuto;
    public primaryColor: number;
    public accentColor: number;

    ngOnInit(): void {
        this.currentTheme = this.themeService.LocalThemeSetting;
        this.primaryColor = Math.floor(this.currentTheme / 3);
        this.accentColor = this.currentTheme % 3;
    }

    public applyTheme() {
        this.changeTheme(this.primaryColor * 3 + this.accentColor);
    }

    public changeTheme(theme: Theme) {
        if (this.currentTheme === theme) {
            return;
        }
        this.currentTheme = theme;
        this.themeService.LocalThemeSetting = theme;
        this.themeService.SetRemoteThemeSetting(theme);
        this.themeService.ApplyTheme(theme);
    }
}
