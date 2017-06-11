import { Component, OnInit } from '@angular/core';

import * as d3 from 'd3';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  private nodes: any[];
  private simulation: any;
  private links: any[];
  private lastNodeId;

  private canvas: HTMLCanvasElement;
  private context: CanvasRenderingContext2D;

  private width: number;
  private height: number;

  ngOnInit(): void {
    this.initCanvas();
    this.initNodes();
    this.initSimulation();
  }

  private initCanvas() {
    this.canvas = document.querySelector('canvas');
    this.context = this.canvas.getContext('2d');
    this.width = this.canvas.width;
    this.height = this.canvas.height;
  }

  private initNodes() {
    this.nodes = [
      { id: 'A', reflexive: false, group: 1 },
      { id: 'B', reflexive: false, group: 1 },
      { id: 'C', reflexive: false, group: 1 }
    ];

    this.lastNodeId = 'C'.charCodeAt(0);
    this.links = [
      { source: this.nodes[0].id, target: this.nodes[1].id, left: false, right: true },
      { source: this.nodes[1].id, target: this.nodes[2].id, left: false, right: true }
    ];
  }

  private initSimulation() {
    this.simulation = d3.forceSimulation()
      .force('link', d3.forceLink().id(function (d) { return d['id']; }))
      .force('charge', d3.forceManyBody())
      .force('center', d3.forceCenter())
      .nodes(this.nodes)
      .on('tick', () => this.ticked());

    this.simulation.force('link')
      .links(this.links);
  }

  private ticked() {
    console.log(this.height);
    this.context.clearRect(0, 0, this.width, this.height);
    this.context.save();
    this.context.translate(this.width / 2, this.height / 2 + 40);

    this.context.beginPath();
    this.links.forEach((d) => this.drawLink(d));
    this.context.strokeStyle = '#aaa';
    this.context.stroke();

    this.context.beginPath();
    this.nodes.forEach((d) => this.drawNode(d));
    this.context.fill();
    this.context.strokeStyle = '#fff';
    this.context.stroke();

    this.context.restore();
  }

  private drawLink(d) {
    this.context.moveTo(d.source.x, d.source.y);
    this.context.lineTo(d.target.x, d.target.y);
  }

private drawNode(d) {
    this.context.moveTo(d.x + 3, d.y);
    this.context.arc(d.x, d.y, 3, 0, 2 * Math.PI);
  }
}
