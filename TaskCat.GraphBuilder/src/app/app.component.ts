import { Component, OnInit, ElementRef, ViewChild } from '@angular/core';
import { Machine } from './machine';

import * as vis from 'vis';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  public machineName = '';
  private machine: Machine;
  private options: any;
  private data: {
    nodes: any[];
    edges: any[];
  };
  private container: any;
  private network: any;
  private nodes: any[];
  private edges: any[];

  @ViewChild('canvasContainer') canvasContainer: ElementRef;
  @ViewChild('fileInput') fileInput: ElementRef;

  ngOnInit(): void {
    this.initNodes();
    this.initContainer();
    this.initWindowResizeEvents();
  }
  private initNodes() {
    this.nodes = [
      { id: 'Pickup', label: 'Pickup' },
      { id: 'Delivery', label: 'Delivery' },
      { id: 'ReturnToSellerDelivery', label: 'ReturnToSellerDelivery' },
      { id: 'ReturnToWarehouseDelivery', label: 'ReturnToWarehouseDelivery' }
    ];

    this.edges = [
      { from: this.nodes[0].id, to: this.nodes[1].id, label: 'COMPLETED', arrows: 'to' },
      { from: this.nodes[0].id, to: this.nodes[0].id, label: 'FAILED', arrows: 'to' },
      { from: this.nodes[1].id, to: this.nodes[2].id, label: 'COMPLETED', arrows: 'to' }
    ];
  }

  public fileChanged($event) {
    this.machineName = '';
    const file = (<HTMLInputElement>this.fileInput.nativeElement).files[0];
    if (file.name.endsWith('.json')) {
      const fileReader = new FileReader();
      fileReader.readAsText(file);
      fileReader.onloadend = (e) => this.onFileLoadEnd(e);
    }
  }

  private onFileLoadEnd($event) {
    const result = $event.target.result;
    const machine = <Machine>JSON.parse(result);
    if (machine) {
      this.machine = machine;
      this.machineName = machine.name;
      this.parseMachine(this.machine);
    }
  }

  private parseMachine(machine: Machine) {
    const nodes = [];
    const edges = [];
    for (let index = 0; index < machine.nodes.length; index++) {
      const node = machine.nodes[index];
      nodes.push({ id: node.type, label: node.type });
    }

    for (let index = 0; index < machine.events.length; index++) {
      const event = machine.events[index];
      edges.push({ from: event.from, to: event.target, label: event.matchCondition.value, arrows: 'to' });
    }

    this.nodes = nodes;
    this.edges = edges;
    this.data = {
      nodes: this.nodes,
      edges: this.edges
    };

    this.network.setData(this.data);
  }

  private initContainer() {
    this.container = this.canvasContainer.nativeElement;
    this.data = {
      nodes: this.nodes,
      edges: this.edges
    };
    this.options = {
      nodes: {
        mass: 10,
        physics: false,
        shape: 'box',
        size: 30,
        font: {
          size: 32
        },
        borderWidth: 2,
        shadow: true
      },
      edges: {
        width: 2,
        length: 100,
        shadow: true,
        font: {
          size: 20,
          align: 'horizontal'
        }
      },
      autoResize: false,
      layout: {
        randomSeed: 2
      },
      interaction: {
        dragNodes: true,
        dragView: false,
        zoomView: false
      }
    };
    this.network = new vis.Network(this.container, this.data, this.options);
  }

  private initWindowResizeEvents() {
    window.addEventListener('resize', () => this.resizeCanvas(), false);
    this.resizeCanvas();
  }

  private resizeCanvas() {
    this.network.setSize(this.canvasContainer.nativeElement.clientWidth, document.documentElement.clientHeight - 250);
    this.network.redraw();
  }
}
