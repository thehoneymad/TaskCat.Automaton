import { Component, OnInit, ElementRef, ViewChild } from '@angular/core';

import * as vis from 'vis';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
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

  ngOnInit(): void {
    this.initNodes();
    this.initContainer();
    this.initWindowResizeEvents();
  }
  private initNodes() {
    this.nodes = [
      { id: 'Pickup', label: 'Pickup'},
      { id: 'Delivery', label: 'Delivery' },
      { id: 'ReturnToSellerDelivery', label: 'ReturnToSellerDelivery'},
      { id: 'ReturnToWarehouseDelivery', label: 'ReturnToWarehouseDelivery'}
    ];

    this.edges = [
      { from: this.nodes[0].id, to: this.nodes[1].id, label: 'COMPLETED', arrows: 'to' },
      { from: this.nodes[0].id, to: this.nodes[0].id, label: 'FAILED', arrows: 'to' },
      { from: this.nodes[1].id, to: this.nodes[2].id, label: 'COMPLETED', arrows: 'to' }
    ];
  }

  private initContainer() {
    this.container = this.canvasContainer.nativeElement;
    this.data = {
      nodes: this.nodes,
      edges: this.edges
    };
    this.options = {
      physics: true,
      nodes: {
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
        shadow: true,
        smooth: {
          type: 'continuous'
        },
        font: {align: 'horizontal'}
      },
      autoResize: false,
      layout: {
        randomSeed: 2,
        hierarchical: {
          enabled: true,
          levelSeparation: 150,
          nodeSpacing: 100,
          treeSpacing: 200,
          blockShifting: true,
          edgeMinimization: true,
          parentCentralization: true,
          direction: 'UD',        // UD, DU, LR, RL
          sortMethod: 'hubsize'   // hubsize, directed
        }
      }
    };
    this.network = new vis.Network(this.container, this.data, this.options);
  }

  private initWindowResizeEvents() {
    window.addEventListener('resize', () => this.resizeCanvas(), false);
    this.resizeCanvas();
  }

  private resizeCanvas() {
    this.network.setSize(this.canvasContainer.nativeElement.clientWidth, this.canvasContainer.nativeElement.clientHeight);
    this.network.redraw();
  }
}
