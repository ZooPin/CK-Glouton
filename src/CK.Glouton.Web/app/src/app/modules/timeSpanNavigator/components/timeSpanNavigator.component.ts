import { Component, Input, OnInit } from '@angular/core';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs/Observable';
import { Subscription } from 'rxjs/Subscription';
import { EffectDispatcher } from '@ck/rx';
import { IAppState } from 'app/app.state';
import { ITimeSpanNavigator, ITimeSpanNavigatorSettings, Scale } from '../models';
import { SubmitTimeSpanEffect } from '../actions';

@Component({
    selector: 'timeSpanNavigator',
    templateUrl: './timeSpanNavigator.component.html'
})
export class TimeSpanNavigatorComponent implements OnInit {

    private _from$: Observable<Date>;
    private _to$: Observable<Date>;
    private _subscriptions: Subscription[];

    private _currentScale: Scale;
    private _currentScaleWidth: number;

    private _range: number[];
    private _dateRange: Date[];

    private _scaleDescription: string;
    private _nextScaleDescription: string;

    get timeSpan(): ITimeSpanNavigator { return this._timeSpan.getValue(); }
    private _timeSpan = new BehaviorSubject<ITimeSpanNavigator>({from: null, to: null});

    @Input()
    configuration: ITimeSpanNavigatorSettings;

    constructor(
        private store: Store<IAppState>,
        private effectDispatcher: EffectDispatcher
    ) {
        this._from$ = store.select(s => s.timeSpanNavigator.from);
        this._to$ = store.select(s => s.timeSpanNavigator.to);
        this._subscriptions = [];
        this._subscriptions.push(this._from$.subscribe(d => this._timeSpan.next({from: d, to: this.timeSpan.to})));
        this._subscriptions.push(this._to$.subscribe(d => this._timeSpan.next({from: this.timeSpan.from, to: d})));
        this._scaleDescription = '';
        this._nextScaleDescription = '';
    }

    /**
     * Returns true if the argument is valid, otherwise false.
     * @param argument The argument passed by the user.
     */
    private validateArgument(argument: any): argument is ITimeSpanNavigatorSettings {
        if(argument === undefined || argument === null) {return false;}
        return (argument as ITimeSpanNavigatorSettings).from !== undefined
            && (argument as ITimeSpanNavigatorSettings).to !== undefined
            && (argument as ITimeSpanNavigatorSettings).scale !== undefined;
    }

    /**
     * Returns the falling and rising edge for the given scale.
     * Throws an error if the given state is invalid.
     * @param scale The scale we want to get falling and rising edges.
     */
    private getEdges(scale: Scale): number[] {
        switch(scale) {
            case Scale.Year: return [1, 100];
            case Scale.Months: return [2, 12];
            case Scale.Days: return [5, 31];
            case Scale.Hours: return [4, 24];
            case Scale.Minutes: return [10, 60];
            case Scale.Seconds: return [1, 60];
            default: throw new Error('Invalid parameter( scale )');
        }
    }

    private getScaleItemPercent(scale: Scale): number {
        return 100 / this.getEdges(scale)[1] * this.getEdges(scale)[0];
    }

    private updateScale(scale: Scale): void {
        if(!(scale in Scale)) {throw new Error('Argument error( scale )');}
        this._currentScale = scale;
        this._currentScaleWidth = this.getScaleItemPercent(this._currentScale);
        this._scaleDescription = `Current scale: ${Scale[this._currentScale]}`;
        this._range = [25, 75]; // Todo: Change me
    }

    /**
     * onChange function
     * @param event The event:
     * e.originalEvent: Slide event
     * e.value: New value
     * e.values: Values in range mode
     */
    private handleChange(event: any): void {
        let state: TimeSpanNavigatorState = TimeSpanNavigatorState.None;
        if(event.values[0] < 5 || event.values[1] > 95) {state |= TimeSpanNavigatorState.Dezoom;}
        if(event.values[1] - event.values[0] < 10) {state |= TimeSpanNavigatorState.Zoom;}
        if(!(this._nextScaleDescription.length === 0)) {state |= TimeSpanNavigatorState.Flagged;}

        switch(state) {
            case TimeSpanNavigatorState.Dezoom:
                if(this._currentScale !== Scale.Year) {
                    this._nextScaleDescription = ` -> ${Scale[this._currentScale - 1]}`;
                    this._scaleDescription += this._nextScaleDescription;
                }
                break;

            case TimeSpanNavigatorState.Zoom:
                if(this._currentScale !== Scale.Seconds) {
                    this._nextScaleDescription = ` -> ${Scale[this._currentScale + 1]}`;
                    this._scaleDescription += this._nextScaleDescription;
                }
                break;

            case TimeSpanNavigatorState.Flagged:
                this._scaleDescription = this._scaleDescription.substring(0,
                    this._scaleDescription.length - (<string>this._nextScaleDescription).length
                );
                this._nextScaleDescription = '';
                break;

            case TimeSpanNavigatorState.None:
            case TimeSpanNavigatorState.Flagged | TimeSpanNavigatorState.Dezoom:
            case TimeSpanNavigatorState.Flagged | TimeSpanNavigatorState.Zoom:
                break;

            default:
                throw new Error(`State invalid.`);
        }
    }

    /**
     * onSlideEnd function
     * @param event The event:
     * event.originalEvent: Mouseup event
     * event.value: New value
     */
    private handleSlideEnd(event: any): void {
        const width: number = this._range[1] - this._range[0];
        if(width < 10) {
            if(this._currentScale !== Scale.Seconds) {this.updateScale(this._currentScale + 1);}
        } else if(this._range[0] <= 5 || this._range[1] >= 95) {
            if(this._currentScale !== Scale.Year) {this.updateScale(this._currentScale - 1);}
        } else {
            const offset: number = (100 - width) / 2;
            this._range = [offset, offset + width];
        }
    }

    /**
     * Initilization method.
     */
    ngOnInit(): void {
        if(!this.validateArgument(this.configuration)) {throw new Error('Configuration is invalid!');}
        this._timeSpan.next({from: new Date(), to: new Date()});
        this._dateRange = [this.configuration.from, this.configuration.to];
        this.updateScale(this.configuration.scale);
    }
}

enum TimeSpanNavigatorState {
    None = 0,
    Dezoom = 1 << 0,
    Zoom = 1 << 1,
    Flagged = 1 << 2
}